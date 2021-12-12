using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Implements an <see cref="IAuthorizationRequirement"/> which requires that
    /// current user from <see cref="AuthorizationContext.User"/> must be authenticated.
    /// </summary>
    public class AuthenticatedUserRequirement : IAuthorizationRequirement
    {
        internal static readonly AuthenticatedUserRequirement Instance = new();

        /// <inheritdoc />
        public Task Authorize(AuthorizationContext context)
        {
            if (context.User == null || !context.User.Identities.Any(x => x.IsAuthenticated))
            {
                context.ReportError("An authenticated user is required.");
            }

            return Task.CompletedTask;
        }
    }
}
