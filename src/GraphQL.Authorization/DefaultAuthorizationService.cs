using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Default implementation of <see cref="IAuthorizationService"/>.
    /// </summary>
    public class DefaultAuthorizationService : IAuthorizationService
    {
        /// <inheritdoc />
        public async Task<AuthorizationResult> AuthorizeAsync(IAuthorizationContext context)
        {
            foreach (var requirement in context.Policy.Requirements)
                await requirement.Authorize(context);

            return context.HasSucceeded
               ? AuthorizationResult.Success()
               : context.HasFailed
                    ? AuthorizationResult.Failed()
                    : AuthorizationResult.Failed(AuthorizationFailure.Failed(context.PendingRequirements));
        }
    }
}
