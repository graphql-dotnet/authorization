using System.Linq;
using System.Threading.Tasks;
using GraphQL.Execution;
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
        private readonly IAuthorizationEvaluator _evaluator;

        /// <summary>
        /// Creates an instance of <see cref="AuthorizationValidationRule"/> with
        /// the specified authorization evaluator.
        /// </summary>
        public AuthorizationValidationRule(IAuthorizationEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        /// <inheritdoc />
        public Task<INodeVisitor> ValidateAsync(ValidationContext context)
        {
            var userContext = context.UserContext as IProvideClaimsPrincipal;
            var operationType = OperationType.Query;

            // this could leak info about hidden fields or types in error messages
            // it would be better to implement a filter on the Schema so it
            // acts as if they just don't exist vs. an auth denied error
            // - filtering the Schema is not currently supported
            // TODO: apply ISchemaFilter - context.Schema.Filter.AllowXXX

            return Task.FromResult((INodeVisitor)new NodeVisitors(
                new MatchingNodeVisitor<Operation>((astType, context) =>
                {
                    operationType = astType.OperationType;

                    var type = context.TypeInfo.GetLastType();
                    CheckAuth(astType, type, userContext, context, operationType);
                }),

                new MatchingNodeVisitor<ObjectField>((objectFieldAst, context) =>
                {
                    if (context.TypeInfo.GetArgument()?.ResolvedType.GetNamedType() is IComplexGraphType argumentType)
                    {
                        var fieldType = argumentType.GetField(objectFieldAst.Name);
                        CheckAuth(objectFieldAst, fieldType, userContext, context, operationType);
                    }
                }),

                new MatchingNodeVisitor<Field>((fieldAst, context) =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    if (fieldDef == null || SkipAuthCheck(fieldAst, context)) 
                        return;

                    // check target field
                    CheckAuth(fieldAst, fieldDef, userContext, context, operationType);
                    // check returned graph type
                    CheckAuth(fieldAst, fieldDef.ResolvedType.GetNamedType(), userContext, context, operationType);
                })
            ));
        }

        private bool SkipAuthCheck(Field field, ValidationContext context)
        {
            if (field.Directives == null || !field.Directives.Any()) return false;

            var operationName = context.OperationName;
            var documentOperations = context.Document.Operations;
            var operation = !string.IsNullOrWhiteSpace(operationName)
                ? documentOperations.WithName(operationName)
                : documentOperations.FirstOrDefault();
            var variables = ExecutionHelper.GetVariableValues(context.Document, context.Schema,
                operation?.Variables, context.Inputs);

            var includeField = GetDirectiveValue(context, field.Directives, DirectiveGraphType.Include, variables);
            if (includeField.HasValue) return !includeField.Value;

            var skipField = GetDirectiveValue(context, field.Directives, DirectiveGraphType.Skip, variables);
            if (skipField.HasValue) return skipField.Value;

            return false;
        }

        private static bool? GetDirectiveValue(ValidationContext context, Directives directives, DirectiveGraphType directiveType, Variables variables)
        {
            var directive = directives.Find(directiveType.Name);
            if (directive == null) return null;

            var argumentValues = ExecutionHelper.GetArgumentValues(
                context.Schema,
                directiveType.Arguments,
                directive.Arguments,
                variables);

            argumentValues.TryGetValue("if", out object ifObj);
            return bool.TryParse(ifObj?.ToString() ?? string.Empty, out bool ifVal) && ifVal;
        }

        private void CheckAuth(
            INode node,
            IProvideMetadata provider,
            IProvideClaimsPrincipal userContext,
            ValidationContext context,
            OperationType? operationType)
        {
            if (provider == null || !provider.RequiresAuthorization())
                return;

            // TODO: async -> sync transition
            var result = _evaluator
                .Evaluate(userContext?.User, context.UserContext, context.Inputs, provider.GetPolicies())
                .GetAwaiter()
                .GetResult();

            if (result.Succeeded)
                return;

            string errors = string.Join("\n", result.Errors);

            context.ReportError(new ValidationError(
                context.Document.OriginalQuery,
                "authorization",
                $"You are not authorized to run this {operationType.ToString().ToLower()}.\n{errors}",
                node));
        }
    }
}
