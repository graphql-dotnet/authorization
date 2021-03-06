using System.Security.Claims;

namespace GraphQL.Authorization
{
    /// <summary>
    /// This interface should be implemented by the object set in <see cref="Validation.ValidationContext.UserContext"/>.
    /// </summary>
    public interface IProvideClaimsPrincipal
    {
        /// <summary>
        /// Gets the current user.
        /// </summary>
        ClaimsPrincipal User { get; }
    }
}
