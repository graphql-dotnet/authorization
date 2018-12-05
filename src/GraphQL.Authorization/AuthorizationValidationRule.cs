using System.Linq;
using System.Threading.Tasks;
using GraphQL.Execution;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    public class AuthorizationValidationRule : IValidationRule
    {
        private readonly IAuthorizationEvaluator _evaluator;

        public AuthorizationValidationRule(IAuthorizationEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public Task<INodeVisitor> ValidateAsync(ValidationContext context)
        {
            var userContext = context.UserContext as IProvideClaimsPrincipal;

            return Task.FromResult((INodeVisitor)new EnterLeaveListener(_ =>
            {
                var operationType = OperationType.Query;

                // this could leak info about hidden fields or types in error messages
                // it would be better to implement a filter on the Schema so it
                // acts as if they just don't exist vs. an auth denied error
                // - filtering the Schema is not currently supported
                // TODO: apply ISchemaFilter - context.Schema.Filter.AllowXXX

                _.Match<Operation>(astType =>
                {
                    operationType = astType.OperationType;

                    var type = context.TypeInfo.GetLastType();
                    CheckAuth(astType, type, userContext, context, operationType);
                });

                _.Match<ObjectField>(objectFieldAst =>
                {
                    if (context.TypeInfo.GetArgument()?.ResolvedType.GetNamedType() is IComplexGraphType argumentType)
                    {
                        var fieldType = argumentType.GetField(objectFieldAst.Name);
                        CheckAuth(objectFieldAst, fieldType, userContext, context, operationType);
                    }
                });

                _.Match<Field>(fieldAst =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    if (fieldDef == null || SkipAuthCheck(fieldAst, context)) 
                        return;

                    // check target field
                    CheckAuth(fieldAst, fieldDef, userContext, context, operationType);
                    // check returned graph type
                    CheckAuth(fieldAst, fieldDef.ResolvedType.GetNamedType(), userContext, context, operationType);
                });
            }));
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
            IProvideMetadata type,
            IProvideClaimsPrincipal userContext,
            ValidationContext context,
            OperationType operationType)
        {
            if (type == null || !type.RequiresAuthorization())
                return;

            // TODO: async -> sync transition
            var result = type
                .Authorize(userContext?.User, context.UserContext, context.Inputs, _evaluator)
                .GetAwaiter()
                .GetResult();

            if (result.Succeeded)
                return;

            string errors = string.Join("\n", result.Errors);

            context.ReportError(new ValidationError(
                context.OriginalQuery,
                "authorization",
                $"You are not authorized to run this {operationType.ToString().ToLower()}.\n{errors}",
                node));
        }
    }
}
