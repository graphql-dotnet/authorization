using System.Security.Claims;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    public class DefaultContextAccessor:IUserContextAccessor
    {
        public ClaimsPrincipal Get(ValidationContext context) {
            if (context.UserContext is IProvideClaimsPrincipal principal)
                return principal.User;
            return null;
        }
    }
}
