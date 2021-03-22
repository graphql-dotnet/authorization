using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Interface to evaluate the authorization result.
    /// </summary>
    public interface IAuthorizationEvaluator
    {
        /// <summary>
        /// Evaluates authorization result.
        /// </summary>
        /// <param name="principal">Represents the current user.</param>
        /// <param name="userContext">Arbitrary user defined context represented as dictionary.</param>
        /// <param name="inputs">Represents a readonly dictionary of variable inputs to an executed document.</param>
        /// <param name="requiredPolicies">A set of policies names to authorize.</param>
        /// <returns></returns>
        Task<AuthorizationResult> Evaluate(
            ClaimsPrincipal principal,
            IDictionary<string, object> userContext,
            IReadOnlyDictionary<string, object> inputs,
            IEnumerable<string> requiredPolicies);
    }
}
