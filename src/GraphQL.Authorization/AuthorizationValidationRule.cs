using System.Linq;
using System.Security.Claims;
using GraphQL.Language.AST;
using GraphQL.Types;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    public class AuthorizationValidationRule : IValidationRule
    {
        private readonly IAuthorizationEvaluator _evaluator;
        private readonly IUserContextAccessor _userContextAccessor;

        public AuthorizationValidationRule(IAuthorizationEvaluator evaluator, IUserContextAccessor userContextAccessor) {
            _evaluator = evaluator;
            _userContextAccessor = userContextAccessor;
        }

        public INodeVisitor Validate(ValidationContext context)
        {
            var claimsPrincipal = _userContextAccessor.Get(context);

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
                    CheckAuth(astType, type, claimsPrincipal, context, operationType);
                });

                _.Match<ObjectField>(objectFieldAst =>
                {
                    if (!(context.TypeInfo.GetArgument().ResolvedType.GetNamedType() is IComplexGraphType argumentType))
                        return;

                    var fieldType = argumentType.GetField(objectFieldAst.Name);
                    CheckAuth(objectFieldAst, fieldType, claimsPrincipal, context, operationType);
                });

                _.Match<Field>(fieldAst =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    if (fieldDef == null) return;

                    // check target field
                    CheckAuth(fieldAst, fieldDef, claimsPrincipal, context, operationType);
                    // check returned graph type
                    CheckAuth(fieldAst, fieldDef.ResolvedType, claimsPrincipal, context, operationType);
                });
            });
        }

        private void CheckAuth(
            INode node,
            IProvideMetadata type,
            ClaimsPrincipal claimsPrincipal,
            ValidationContext context,
            OperationType operationType)
        {
            if (type == null || !type.RequiresAuthorization()) return;

            var result = type
                .Authorize(claimsPrincipal, context.UserContext, context.Inputs, _evaluator)
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
