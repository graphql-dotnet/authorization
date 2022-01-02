using System.Linq;
using System.Threading.Tasks;
using GraphQL.Language.AST;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Skips authorization checks for introspection queries, namely all queries
    /// that contain only __schema, __type and __typename top-level fields.
    /// </summary>
    public class IntrospectionSkipCondition : IAuthorizationSkipCondition
    {
        /// <inheritdoc />
        public ValueTask<bool> ShouldSkip(ValidationContext context)
        {
            static bool IsIntrospectionField(Field f) => f.Name == "__schema" || f.Name == "__type" || f.Name == "__typename";

            bool ContainsOnlyIntrospectionFields(IHaveSelectionSet node)
            {
                if (node.SelectionSet?.Selections?.Count == 0)
                    return false; // invalid document, better to return false

                foreach (var selection in node.SelectionSet!.Selections)
                {
                    switch (selection)
                    {
                        case Field field:
                            if (!IsIntrospectionField(field))
                                return false;
                            break;

                        case InlineFragment inlineFragment:
                            if (!ContainsOnlyIntrospectionFields(inlineFragment))
                                return false;
                            break;

                        case FragmentSpread fragmentSpread:
                            var fragmentDef = context.Document.Fragments.FindDefinition(fragmentSpread.Name);
                            if (fragmentDef == null || !ContainsOnlyIntrospectionFields(fragmentDef))
                                return false;
                            break;

                        default:
                            return false;
                    }
                }

                return true;
            }

            var actualOperation = context.Document.Operations.FirstOrDefault(x => x.Name == context.OperationName) ?? context.Document.Operations.FirstOrDefault();

            return new ValueTask<bool>(actualOperation?.OperationType == OperationType.Query
                ? ContainsOnlyIntrospectionFields(actualOperation)
                : false); // not an executable document
        }
    }
}
