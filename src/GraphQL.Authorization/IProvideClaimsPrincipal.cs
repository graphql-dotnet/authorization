using System.Security.Claims;

namespace GraphQL.Authorization
{
    public interface IProvideClaimsPrincipal
    {
        ClaimsPrincipal User { get; }
    }
}