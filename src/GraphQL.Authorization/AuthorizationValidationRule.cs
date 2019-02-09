using System.Collections.Generic;
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

                _.Match<Operation>(astType =>
                {
                    operationType = astType.OperationType;

                    var type = context.TypeInfo.GetLastType();
                    CheckAuth(astType, type, userContext, context, operationType);
                });

                _.Match<ObjectField>(objectFieldAst =>
                {
                    var argumentType = context.TypeInfo.GetArgument().ResolvedType.GetNamedType() as IComplexGraphType;
                    if (argumentType == null)
                        return;

                    var fieldType = argumentType.GetField(objectFieldAst.Name);
                    CheckAuth(objectFieldAst, fieldType, userContext, context, operationType);
                });

                _.Match<Field>(fieldAst =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    if (fieldDef == null) return;

                    // check target field
                    CheckAuth(fieldAst, fieldDef, userContext, context, operationType);
                    // check returned graph type
                    CheckAuth(fieldAst, fieldDef.ResolvedType.GetNamedType(), userContext, context, operationType);
                });

                _.Match<VariableReference>(variable =>
                {
                    var variableType = context.TypeInfo.GetArgument().ResolvedType.GetNamedType() as IComplexGraphType;
                    if (variableType == null)
                        return;

                    CheckAuth(variable, variableType, userContext, context, operationType);

                    // check authorization for each existing field in the Variable
                    if (context.Inputs.TryGetValue(variable.Name, out var fields) &&
                            fields is Dictionary<string, object> fieldsValues)
                    {
                        foreach (var field in variableType.Fields)
                        {
                            if (fieldsValues.ContainsKey(field.Name))
                            {
                                CheckAuth(variable, field, userContext, context, operationType);
                            }
                        }
                    }
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
    }
}
