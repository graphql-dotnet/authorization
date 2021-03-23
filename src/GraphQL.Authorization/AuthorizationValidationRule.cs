using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    /// <summary>
    /// GraphQL authorization validation rule which evaluates configured
    /// (via policies) requirements on schema elements: types, fields, etc.
    /// </summary>
    public class AuthorizationValidationRule : IValidationRule
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IClaimsPrincipalAccessor _claimsPrincipalAccessor;
        private readonly IAuthorizationPolicyProvider _policyProvider;

        /// <summary>
        /// Creates an instance of <see cref="AuthorizationValidationRule"/> with
        /// the specified values.
        /// </summary>
        public AuthorizationValidationRule(IAuthorizationService authorizationService, IClaimsPrincipalAccessor claimsPrincipalAccessor, IAuthorizationPolicyProvider policyProvider)
        {
            _authorizationService = authorizationService;
            _claimsPrincipalAccessor = claimsPrincipalAccessor;
            _policyProvider = policyProvider;
        }

        /// <inheritdoc />
        public async Task<INodeVisitor> ValidateAsync(ValidationContext context)
        {
            await AuthorizeAsync(null, context.Schema, context, null);
            var operationType = OperationType.Query;

            // this could leak info about hidden fields or types in error messages
            // it would be better to implement a filter on the Schema so it
            // acts as if they just don't exist vs. an auth denied error
            // - filtering the Schema is not currently supported
            // TODO: apply ISchemaFilter - context.Schema.Filter.AllowXXX

            return new NodeVisitors(
                new MatchingNodeVisitor<Operation>((astType, context) =>
                {
                    operationType = astType.OperationType;

                    var type = context.TypeInfo.GetLastType();
                    AuthorizeAsync(astType, type, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this;
                }),

                new MatchingNodeVisitor<ObjectField>((objectFieldAst, context) =>
                {
                    if (context.TypeInfo.GetArgument()?.ResolvedType.GetNamedType() is IComplexGraphType argumentType)
                    {
                        var fieldType = argumentType.GetField(objectFieldAst.Name);
                        AuthorizeAsync(objectFieldAst, fieldType, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this;
                    }
                }),

                new MatchingNodeVisitor<Field>((fieldAst, context) =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    if (fieldDef == null)
                        return;

                    // check target field
                    AuthorizeAsync(fieldAst, fieldDef, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this;
                    // check returned graph type
                    AuthorizeAsync(fieldAst, fieldDef.ResolvedType.GetNamedType(), context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this;
                })
            );
        }

        /// <summary>
        /// Creates authorization context to pass to <see cref="IAuthorizationService.AuthorizeAsync(IAuthorizationContext)"/>.
        /// </summary>
        /// <param name="context">GraphQL validation context.</param>
        /// <param name="policyName">Name of checked policy for the current authorization processing.</param>
        protected virtual IAuthorizationContext CreateAuthorizationContext(ValidationContext context, string policyName)
        {
            if (policyName == null)
                throw new ArgumentNullException(nameof(policyName));

            return new DefaultAuthorizationContext(
                _policyProvider.GetPolicy(policyName) ?? new AuthorizationPolicy(new PolicyDefinedRequirement(policyName)),
                _claimsPrincipalAccessor.GetClaimsPrincipal(context) ?? new ClaimsPrincipal(new ClaimsIdentity()))
            {
                UserContext = context.UserContext,
                Inputs = context.Inputs,
            };
        }

        private async Task AuthorizeAsync(INode node, IProvideMetadata provider, ValidationContext context, OperationType? operationType)
        {
            var policyNames = provider?.GetPolicies();

            if (policyNames?.Count == 1)
            {
                // small optimization for the single policy - no 'new List<>()', no 'await Task.WhenAll()'
                var authorizationResult = await _authorizationService.AuthorizeAsync(CreateAuthorizationContext(context, policyNames[0]));
                if (!authorizationResult.Succeeded)
                    AddValidationError(node, context, operationType, authorizationResult);
            }
            else if (policyNames?.Count > 1)
            {
                var tasks = new List<Task<AuthorizationResult>>(policyNames.Count);
                foreach (string policyName in policyNames)
                {
                    var task = _authorizationService.AuthorizeAsync(CreateAuthorizationContext(context, policyName));
                    tasks.Add(task);
                }

                var authorizationResults = await Task.WhenAll(tasks);

                foreach (var result in authorizationResults)
                {
                    if (!result.Succeeded)
                        AddValidationError(node, context, operationType, result);
                }
            }
        }

        /// <summary>
        /// Adds an authorization failure error to the document response.
        /// </summary>
        protected virtual void AddValidationError(INode node, ValidationContext context, OperationType? operationType, AuthorizationResult result)
        {
            context.ReportError(new AuthorizationError(node, context, operationType, result));
        }
    }
}
