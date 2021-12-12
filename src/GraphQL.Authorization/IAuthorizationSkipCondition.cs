using System.Threading.Tasks;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Allows to conditionally skip entire AST traversing and all
    /// authorization checks in <see cref="AuthorizationValidationRule"/>.
    /// </summary>
    public interface IAuthorizationSkipCondition
    {
        /// <summary>
        /// Specifies whether authorization checks should be skipped.
        /// </summary>
        ValueTask<bool> ShouldSkip(ValidationContext context);
    }
}
