using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    public class AuthenticatedUserRequirement : IAuthorizationRequirement
    {
        internal static readonly AuthenticatedUserRequirement Instance = new AuthenticatedUserRequirement();

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
