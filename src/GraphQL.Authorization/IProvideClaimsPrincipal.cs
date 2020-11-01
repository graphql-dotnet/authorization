using System.Security.Claims;

namespace GraphQL.Authorization
{
    /// <summary>
    /// This interface should be implemented by object in <see cref="Validation.ValidationContext.UserContext"/>.
    /// </summary>
    public interface IProvideClaimsPrincipal
    {
        ClaimsPrincipal User { get; }
    }
}
