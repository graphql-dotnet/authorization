using System.Collections.Generic;
using System.Linq;
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
        private readonly IAuthorizationEvaluator _evaluator;

        /// <summary>
        /// Creates an instance of <see cref="AuthorizationValidationRule"/> with
        /// the specified authorization evaluator.
        /// </summary>
        public AuthorizationValidationRule(IAuthorizationEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        private bool ShouldBeSkipped(Operation actualOperation, ValidationContext context)
        {
            if (context.Document.Operations.Count <= 1)
            {
                return false;
            }

            int i = 0;
            do
            {
                var ancestor = context.TypeInfo.GetAncestor(i++);

                if (ancestor == actualOperation)
                {
                    return false;
                }

                if (ancestor == context.Document)
                {
                    return true;
                }

                if (ancestor is FragmentDefinition fragment)
                {
                    return !FragmentBelongsToOperation(fragment, actualOperation);
                }
            } while (true);
        }

        private bool FragmentBelongsToOperation(FragmentDefinition fragment, Operation operation)
        {
            bool belongs = false;
            void Visit(INode node, int _)
            {
                if (belongs)
                {
                    return;
                }

                belongs = node is FragmentSpread fragmentSpread && fragmentSpread.Name == fragment.Name;

                if (node != null)
                {
                    node.Visit(Visit, 0);
                }
            }

            operation.Visit(Visit, 0);

            return belongs;
        }

        /// <inheritdoc />
        public ValueTask<INodeVisitor?> ValidateAsync(ValidationContext context)
        {
            var userContext = context.UserContext as IProvideClaimsPrincipal;
            var operationType = OperationType.Query;
            var actualOperation = context.Document.Operations.FirstOrDefault(x => x.Name == context.OperationName) ?? context.Document.Operations.FirstOrDefault();

            // this could leak info about hidden fields or types in error messages
            // it would be better to implement a filter on the Schema so it
            // acts as if they just don't exist vs. an auth denied error
            // - filtering the Schema is not currently supported
            // TODO: apply ISchemaFilter - context.Schema.Filter.AllowXXX
            return new ValueTask<INodeVisitor?>(new NodeVisitors(
                new MatchingNodeVisitor<Operation>((astType, context) =>
                {
                    if (context.Document.Operations.Count > 1 && astType.Name != context.OperationName)
                    {
                        return;
                    }

                    operationType = astType.OperationType;

                    var type = context.TypeInfo.GetLastType();
                    CheckAuth(astType, type, userContext, context, operationType);
                }),

                new MatchingNodeVisitor<ObjectField>((objectFieldAst, context) =>
                {
                    if (context.TypeInfo.GetArgument()?.ResolvedType?.GetNamedType() is IComplexGraphType argumentType && !ShouldBeSkipped(actualOperation, context))
                    {
                        var fieldType = argumentType.GetField(objectFieldAst.Name);
                        CheckAuth(objectFieldAst, fieldType, userContext, context, operationType);
                    }
                }),

                new MatchingNodeVisitor<Field>((fieldAst, context) =>
                {
                    var fieldDef = context.TypeInfo.GetFieldDef();

                    if (fieldDef == null || ShouldBeSkipped(actualOperation, context))
                        return;

                    // check target field
                    CheckAuth(fieldAst, fieldDef, userContext, context, operationType);
                    // check returned graph type
                    CheckAuth(fieldAst, fieldDef.ResolvedType?.GetNamedType(), userContext, context, operationType);
                }),

                new MatchingNodeVisitor<VariableReference>((variableRef, context) =>
                {
                    if (!(context.TypeInfo.GetArgument()?.ResolvedType?.GetNamedType() is IComplexGraphType variableType))
                        return;

                    CheckAuth(variableRef, variableType, userContext, context, operationType);

                    // Check each supplied field in the variable that exists in the variable type.
                    // If some supplied field does not exist in the variable type then some other
                    // validation rule should check that but here we should just ignore that
                    // "unknown" field.
                    if (context.Inputs != null &&
                        context.Inputs.TryGetValue(variableRef.Name, out object input) &&
                        input is Dictionary<string, object> fieldsValues)
                    {
                        foreach (var field in variableType.Fields)
                        {
                            if (fieldsValues.ContainsKey(field.Name))
                            {
                                CheckAuth(variableRef, field, userContext, context, operationType);
                            }
                        }
                    }
                })
            ));
        }

        private void CheckAuth(
            INode node,
            IProvideMetadata? provider,
            IProvideClaimsPrincipal? userContext,
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
                context.Document.OriginalQuery!, // TODO: ???
                "authorization",
                $"You are not authorized to run this {operationType.ToString().ToLower()}.\n{errors}",
                node));
        }
    }
}
