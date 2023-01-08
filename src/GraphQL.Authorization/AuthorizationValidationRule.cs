using GraphQL.Types;
using GraphQL.Validation;
using GraphQLParser;
using GraphQLParser.AST;
using GraphQLParser.Visitors;

namespace GraphQL.Authorization;

/// <summary>
/// GraphQL authorization validation rule which evaluates configured
/// (via policies) requirements on schema elements: types, fields, etc.
/// </summary>
public class AuthorizationValidationRule : IValidationRule
{
    private readonly Visitor _visitor;

    /// <summary>
    /// Creates an instance of <see cref="AuthorizationValidationRule"/> with
    /// the specified authorization evaluator.
    /// </summary>
    public AuthorizationValidationRule(IAuthorizationEvaluator evaluator)
    {
        _visitor = new(evaluator);
    }

    /// <inheritdoc />
    public async ValueTask<INodeVisitor?> ValidateAsync(ValidationContext context)
    {
        await _visitor.AuthorizeAsync(null, context.Schema, context).ConfigureAwait(false);

        // this could leak info about hidden fields or types in error messages
        // it would be better to implement a filter on the Schema so it
        // acts as if they just don't exist vs. an auth denied error
        // - filtering the Schema is not currently supported
        // TODO: apply ISchemaFilter - context.Schema.Filter.AllowXXX
        return _visitor;
    }

    private class Visitor : INodeVisitor
    {
        private readonly IAuthorizationEvaluator _evaluator;

        public Visitor(IAuthorizationEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        public async ValueTask EnterAsync(ASTNode node, ValidationContext context)
        {
            if (node is GraphQLOperationDefinition && node == context.Operation)
            {
                var type = context.TypeInfo.GetLastType();
                await AuthorizeAsync(node, type, context).ConfigureAwait(false);
            }

            if (node is GraphQLObjectField objectFieldAst &&
                context.TypeInfo.GetArgument()?.ResolvedType?.GetNamedType() is IComplexGraphType argumentType &&
                !await ShouldBeSkipped(context.Operation, context).ConfigureAwait(false))
            {
                var fieldType = argumentType.GetField(objectFieldAst.Name);
                await AuthorizeAsync(objectFieldAst, fieldType, context).ConfigureAwait(false);
            }

            if (node is GraphQLField fieldAst)
            {
                var fieldDef = context.TypeInfo.GetFieldDef();

                if (fieldDef == null || await ShouldBeSkipped(context.Operation, context).ConfigureAwait(false))
                    return;

                // check target field
                await AuthorizeAsync(fieldAst, fieldDef, context).ConfigureAwait(false);
                // check returned graph type
                await AuthorizeAsync(fieldAst, fieldDef.ResolvedType?.GetNamedType(), context).ConfigureAwait(false);
            }

            if (node is GraphQLVariable variableRef)
            {
                if (context.TypeInfo.GetArgument()?.ResolvedType?.GetNamedType() is not IComplexGraphType variableType ||
                    await ShouldBeSkipped(context.Operation, context).ConfigureAwait(false))
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

        public ValueTask LeaveAsync(ASTNode node, ValidationContext context) => default;

        private async ValueTask<bool> ShouldBeSkipped(GraphQLOperationDefinition actualOperation, ValidationContext context)
        {
            if (context.Document.OperationsCount() <= 1)
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

                if (ancestor is GraphQLFragmentDefinition fragment)
                {
                    //TODO: may be rewritten completely later
                    var c = new FragmentBelongsToOperationVisitorContext(fragment);
                    await _fragmentBelongsToOperationVisitor.VisitAsync(actualOperation, c).ConfigureAwait(false);
                    return !c.Found;
                }
            } while (true);
        }

        public async ValueTask AuthorizeAsync(
            ASTNode? node,
            IProvideMetadata? provider,
            ValidationContext context)
        {
            var userContext = context.UserContext as IProvideClaimsPrincipal;
            var user = userContext == null ? context.User : userContext.User;
            var operationType = context.Operation.Operation;

            if (provider == null || !provider.IsAuthorizationRequired())
                return;

            var result = await _evaluator.Evaluate(user, context.UserContext, context.Variables, provider.GetPolicies()).ConfigureAwait(false);

            if (result.Succeeded)
                return;

            string errors = string.Join("\n", result.Errors);

            context.ReportError(new ValidationError(
                context.Document.Source,
                "authorization",
                $"You are not authorized to run this {operationType.ToString().ToLower()}.\n{errors}",
                node == null ? Array.Empty<ASTNode>() : new ASTNode[] { node }));
        }
    }

    private sealed class FragmentBelongsToOperationVisitorContext : IASTVisitorContext
    {
        public FragmentBelongsToOperationVisitorContext(GraphQLFragmentDefinition fragment)
        {
            Fragment = fragment;
        }

        public GraphQLFragmentDefinition Fragment { get; }

        public bool Found { get; set; }

        public CancellationToken CancellationToken => default;
    }

    private static readonly FragmentBelongsToOperationVisitor _fragmentBelongsToOperationVisitor = new();

    private sealed class FragmentBelongsToOperationVisitor : ASTVisitor<FragmentBelongsToOperationVisitorContext>
    {
        protected override ValueTask VisitFragmentSpreadAsync(GraphQLFragmentSpread fragmentSpread, FragmentBelongsToOperationVisitorContext context)
        {
            context.Found = context.Fragment.FragmentName.Name == fragmentSpread.FragmentName.Name;
            return default;
        }

        public override ValueTask VisitAsync(ASTNode? node, FragmentBelongsToOperationVisitorContext context)
        {
            return context.Found ? default : base.VisitAsync(node, context);
        }
    }
}
