using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;
using System.Collections.Generic;

namespace GraphQL.Authorization
{
    public class AuthorizationValidationRule : IValidationRule
    {
        private readonly IAuthorizationEvaluator _evaluator;

        public AuthorizationValidationRule(IAuthorizationEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public INodeVisitor Validate(ValidationContext context)
        {
            var userContext = context.UserContext as IProvideClaimsPrincipal;

            return new EnterLeaveListener(_ =>
            {
                var operationType = OperationType.Query;

                // this could leak info about hidden fields or types in error messages
                // it would be better to implement a filter on the Schema so it
                // acts as if they just don't exist vs. an auth denied error
                // - filtering the Schema is not currently supported

                _.Match<ObjectField>(objectFieldAst =>
                {
                    var argumentType = context.TypeInfo.GetArgument().ResolvedType.GetNamedType() as IComplexGraphType;
                    if (argumentType == null)
                    {
                        return;
                    }

                    var fieldType = argumentType.GetField(objectFieldAst.Name);
                    if (fieldType.PublicAuthorization())
                    {
                        return;
                    }

                    CheckAuth(objectFieldAst, fieldType, userContext, context, operationType);
                });

                _.Match<Field>(fieldAst =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();
                    if (fieldDef == null || fieldDef.PublicAuthorization())
                    {
                        return;
                    }

                    // check target field
                    CheckAuth(fieldAst, fieldDef, userContext, context, operationType);
                    // check returned graph type
                    CheckAuth(fieldAst, fieldDef.ResolvedType.GetNamedType(), userContext, context, operationType);
                });
            });
        }

        private void CheckAuth(
            INode node,
            IProvideMetadata type,
            IProvideClaimsPrincipal userContext,
            ValidationContext context,
            OperationType operationType)
        {
            InheritParentPolicies(type, operationType, context);

            if (type == null || !type.RequiresAuthorization()) return;

            var result = type
                .Authorize(userContext?.User, context.UserContext, context.Inputs, _evaluator)
                .GetAwaiter()
                .GetResult();

            if (result.Succeeded) return;

            var errors = string.Join("\n", result.Errors);

            context.ReportError(new ValidationError(
                context.OriginalQuery,
                "authorization",
                $"You are not authorized to run this {operationType.ToString().ToLower()}.\n{errors}",
                node));
        }

        private void InheritParentPolicies(IProvideMetadata type, OperationType operationType, ValidationContext context)
        {
            List<string> parentPolicies = new List<string>();

            if (operationType == OperationType.Query)
            {
                parentPolicies = context.Schema.Query.GetPolicies();
            }
            else if (operationType == OperationType.Mutation)
            {
                parentPolicies = context.Schema.Mutation.GetPolicies();
            }
            else if (operationType == OperationType.Subscription)
            {
                parentPolicies = context.Schema.Subscription.GetPolicies();
            }

            if (parentPolicies.Count > 0)
            {
                type.AuthorizeWith(parentPolicies.ToArray());
            }
        }
    }
}
