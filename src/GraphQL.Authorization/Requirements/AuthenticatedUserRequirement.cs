using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Implements an <see cref="IAuthorizationRequirement"/> which requires that
    /// current user from <see cref="IAuthorizationContext.User"/> must be authenticated.
    /// </summary>
    public class AuthenticatedUserRequirement : IAuthorizationRequirement
    {
        internal static readonly AuthenticatedUserRequirement Instance = new AuthenticatedUserRequirement();

        /// <inheritdoc />
        public Task Authorize(IAuthorizationContext context)
        {
            if (context.User != null && context.User.Identities.Any(x => x.IsAuthenticated))
                context.Succeed(this);

            return Task.CompletedTask;
        }
    }
}
