using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Represents an authorization requirement.
    /// One of the requirements in <see cref="IAuthorizationPolicy"/>.
    /// </summary>
    public interface IAuthorizationRequirement
    {
        /// <summary>
        /// Execute requirement. If the requirement is met then this method
        /// should call <see cref="IAuthorizationContext.Succeed(IAuthorizationRequirement)"/>.
        /// </summary>
        Task Authorize(IAuthorizationContext context);
    }

    /// <summary>
    /// Represents an authorization requirement that provides error message to include
    /// in resulting GraphQL error.
    /// </summary>
    public interface IAuthorizationRequirementWithErrorMessage : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets error message.
        /// </summary>
        string ErrorMessage { get; }
    }
}
