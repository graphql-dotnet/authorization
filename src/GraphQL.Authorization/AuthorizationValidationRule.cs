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
    private readonly IAuthorizationEvaluator _evaluator;

    /// <summary>
    /// Creates an instance of <see cref="AuthorizationValidationRule"/> with
    /// the specified authorization evaluator.
    /// </summary>
    public AuthorizationValidationRule(IAuthorizationEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    private bool ShouldBeSkipped(GraphQLOperationDefinition actualOperation, ValidationContext context)
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
                _visitor.VisitAsync(actualOperation, c).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this
                return !c.Found;
            }
        } while (true);
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

    private static readonly FragmentBelongsToOperationVisitor _visitor = new();

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

    /// <inheritdoc />
    public async ValueTask<INodeVisitor?> ValidateAsync(ValidationContext context)
    {
        var userContext = context.UserContext as IProvideClaimsPrincipal;
        await AuthorizeAsync(null, context.Schema, userContext, context, null).ConfigureAwait(false);
        var operationType = OperationType.Query;

        // this could leak info about hidden fields or types in error messages
        // it would be better to implement a filter on the Schema so it
        // acts as if they just don't exist vs. an auth denied error
        // - filtering the Schema is not currently supported
        // TODO: apply ISchemaFilter - context.Schema.Filter.AllowXXX
        return new NodeVisitors(
            new MatchingNodeVisitor<GraphQLOperationDefinition>((astType, context) =>
            {
                if (context.Document.OperationsCount() > 1 && astType.Name != context.Operation.Name)
                {
                    return;
                }

                operationType = astType.Operation;

                var type = context.TypeInfo.GetLastType();
                AuthorizeAsync(astType, type, userContext, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this
            }),

            new MatchingNodeVisitor<GraphQLObjectField>((objectFieldAst, context) =>
            {
                if (context.TypeInfo.GetArgument()?.ResolvedType?.GetNamedType() is IComplexGraphType argumentType && !ShouldBeSkipped(context.Operation, context))
                {
                    var fieldType = argumentType.GetField(objectFieldAst.Name);
                    AuthorizeAsync(objectFieldAst, fieldType, userContext, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this
                }
            }),

            new MatchingNodeVisitor<GraphQLField>((fieldAst, context) =>
            {
                var fieldDef = context.TypeInfo.GetFieldDef();

                if (fieldDef == null || ShouldBeSkipped(context.Operation, context))
                    return;

                // check target field
                AuthorizeAsync(fieldAst, fieldDef, userContext, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this
                // check returned graph type
                AuthorizeAsync(fieldAst, fieldDef.ResolvedType?.GetNamedType(), userContext, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this
            }),

            new MatchingNodeVisitor<GraphQLVariable>((variableRef, context) =>
            {
                if (context.TypeInfo.GetArgument()?.ResolvedType?.GetNamedType() is not IComplexGraphType variableType || ShouldBeSkipped(context.Operation, context))
                    return;

                AuthorizeAsync(variableRef, variableType, userContext, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this

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
                            AuthorizeAsync(variableRef, field, userContext, context, operationType).GetAwaiter().GetResult(); // TODO: need to think of something to avoid this
                        }
                    }
                }
            })
        );
    }

    private async Task AuthorizeAsync(
        ASTNode? node,
        IProvideMetadata? provider,
        IProvideClaimsPrincipal? userContext,
        ValidationContext context,
        OperationType? operationType)
    {
        if (provider == null || !provider.IsAuthorizationRequired())
            return;

        var result = await _evaluator.Evaluate(userContext?.User, context.UserContext, context.Variables, provider.GetPolicies()).ConfigureAwait(false);

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
