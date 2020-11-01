using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    public interface IAuthorizationEvaluator
    {
        Task<AuthorizationResult> Evaluate(
            ClaimsPrincipal principal,
            IDictionary<string, object> userContext,
            Inputs arguments,
            IEnumerable<string> requiredPolicies);
    }
}
