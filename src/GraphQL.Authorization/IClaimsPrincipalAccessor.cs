using System.Security.Claims;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Provides access to the <see cref="ClaimsPrincipal"/> used when authorizing a GraphQL operation.
    /// </summary>
    public interface IClaimsPrincipalAccessor
    {
        /// <summary>
        /// Provides the <see cref="ClaimsPrincipal"/> for the current <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext"/> of the current operation.</param>
        /// <returns></returns>
        ClaimsPrincipal? GetClaimsPrincipal(ValidationContext context);
    }
}
