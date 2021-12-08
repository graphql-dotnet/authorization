using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Implements an <see cref="IAuthorizationRequirement"/> which requires that
    /// the specified policy must be defined.
    /// </summary>
    public class DefinedPolicyRequirement : IAuthorizationRequirementWithErrorMessage
    {
        /// <summary>
        /// Creates a new instance of <see cref="DefinedPolicyRequirement"/> with
        /// the specified (undefined) policy name.
        /// </summary>
        public DefinedPolicyRequirement(string policyName)
        {
            PolicyName = policyName;
        }

        /// <summary>
        /// Gets name of the undefined policy.
        /// </summary>
        public string PolicyName { get; }

        /// <summary>
        /// Execute requirement. This requirement always isn't met by design.
        /// </summary>
        public Task Authorize(IAuthorizationContext _) => Task.CompletedTask;

        /// <inheritdoc />
        public string ErrorMessage => $"Required policy '{PolicyName}' is not present.";
    }
}
