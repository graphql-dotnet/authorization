using System.Security.Claims;

namespace GraphQL.Authorization
{
    /// <summary>
    /// This interface should be implemented by <see cref="GraphQL.Validation.ValidationContext.UserContext"/>.
    /// </summary>
    public interface IProvideClaimsPrincipal
    {
        ClaimsPrincipal User { get; }
    }
}
