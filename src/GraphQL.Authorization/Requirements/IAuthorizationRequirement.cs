using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary> One of the requirements in <see cref="IAuthorizationPolicy"/>. </summary>
    public interface IAuthorizationRequirement
    {
        /// <summary>
        /// Execute requirement. If the requirement is not met then this method should call <see cref="AuthorizationContext.ReportError(string)"/>.
        /// </summary>
        Task Authorize(AuthorizationContext context);
    }
}
