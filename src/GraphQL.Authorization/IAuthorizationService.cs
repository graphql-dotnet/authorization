using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Interface to evaluate the authorization result.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Evaluates authorization result.
        /// </summary>
        /// <param name="context">Provides context information to evaluate the authorization result.</param>
        Task<AuthorizationResult> AuthorizeAsync(IAuthorizationContext context);
    }
}
