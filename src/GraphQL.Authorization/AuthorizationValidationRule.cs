using GraphQL.Types;
using GraphQL.Validation;
using GraphQLParser.AST;

namespace GraphQL.Authorization;

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
    public async ValueTask<INodeVisitor?> ValidateAsync(ValidationContext context)
    {
        var visitor = new Visitor(_evaluator);

        await visitor.AuthorizeAsync(null, context.Schema, context).ConfigureAwait(false);

        // this could leak info about hidden fields or types in error messages
        // it would be better to implement a filter on the Schema so it
        // acts as if they just don't exist vs. an auth denied error
        // - filtering the Schema is not currently supported
        // TODO: apply ISchemaFilter - context.Schema.Filter.AllowXXX
        return visitor;
    }

    private class Visitor : INodeVisitor
    {
        private readonly IAuthorizationEvaluator _evaluator;
        private bool _validate;

        public Visitor(IAuthorizationEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public async ValueTask EnterAsync(ASTNode node, ValidationContext context)
        {
            if ((node is GraphQLOperationDefinition astType && astType == context.Operation) ||
                (node is GraphQLFragmentDefinition fragment && (context.GetRecursivelyReferencedFragments(context.Operation)?.Contains(fragment) ?? false)))
            {
                var type = context.TypeInfo.GetLastType();
                await AuthorizeAsync(node, type, context).ConfigureAwait(false);
                _validate = true;
            }

            if (!_validate)
                return;

            if (node is GraphQLObjectField objectFieldAst &&
                context.TypeInfo.GetArgument()?.ResolvedType?.GetNamedType() is IComplexGraphType argumentType)
            {
                var fieldType = argumentType.GetField(objectFieldAst.Name);
                await AuthorizeAsync(objectFieldAst, fieldType, context).ConfigureAwait(false);
            }

            if (node is GraphQLField fieldAst)
            {
                var fieldDef = context.TypeInfo.GetFieldDef();

                if (fieldDef == null)
                    return;

                // check target field
                await AuthorizeAsync(fieldAst, fieldDef, context).ConfigureAwait(false);
                // check returned graph type
                await AuthorizeAsync(fieldAst, fieldDef.ResolvedType?.GetNamedType(), context).ConfigureAwait(false);
            }

            if (node is GraphQLVariable variableRef)
            {
                if (context.TypeInfo.GetArgument()?.ResolvedType?.GetNamedType() is not IComplexGraphType variableType)
                    return;

                await AuthorizeAsync(variableRef, variableType, context).ConfigureAwait(false);

                // Check each supplied field in the variable that exists in the variable type.
                // If some supplied field does not exist in the variable type then some other
                // validation rule should check that but here we should just ignore that
                // "unknown" field.
                if (context.Variables != null &&
                    context.Variables.TryGetValue(variableRef.Name.StringValue, out object? input) && //ISSUE:allocation
                    input is Dictionary<string, object> fieldsValues)
                {
                    foreach (var field in variableType.Fields)
                    {
                        if (fieldsValues.ContainsKey(field.Name))
                        {
                            await AuthorizeAsync(variableRef, field, context).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        public ValueTask LeaveAsync(ASTNode node, ValidationContext context)
        {
            if (node is GraphQLOperationDefinition || node is GraphQLFragmentDefinition)
                _validate = false;

            return default;
        }

        public async ValueTask AuthorizeAsync(ASTNode? node, IProvideMetadata? provider, ValidationContext context)
        {
            if (provider == null || !provider.IsAuthorizationRequired())
                return;

            var result = await _evaluator.Evaluate(context.User, context.UserContext, context.Variables, provider.GetPolicies()).ConfigureAwait(false);

            if (result.Succeeded)
                return;

            string errors = string.Join("\n", result.Errors);

            context.ReportError(new ValidationError(
                context.Document.Source,
                "authorization",
                $"You are not authorized to run this {context.Operation.Operation.ToString().ToLower()}.\n{errors}",
                node == null ? Array.Empty<ASTNode>() : new ASTNode[] { node }));
        }
    }
}
