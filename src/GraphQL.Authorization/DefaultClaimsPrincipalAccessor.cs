using System.Security.Claims;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    /// <summary>
    /// The default claims principal accessor.
    /// </summary>
    public class DefaultClaimsPrincipalAccessor : IClaimsPrincipalAccessor
    {
        /// <inheritdoc />
        public ClaimsPrincipal? GetClaimsPrincipal(ValidationContext context) => (context.UserContext as IProvideClaimsPrincipal)?.User;
    }
}
