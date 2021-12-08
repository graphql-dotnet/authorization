using System.Security.Claims;

namespace GraphQL.Authorization
{
    /// <summary>
    /// This interface should be implemented by the object set in <see cref="Validation.ValidationContext.UserContext"/>
    /// to provide current user for which all the configured policies will be evaluated.
    /// </summary>
    public interface IProvideClaimsPrincipal
    {
        /// <summary>
        /// Gets the current user.
        /// </summary>
        ClaimsPrincipal? User { get; }
    }
}
