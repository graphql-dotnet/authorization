using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    public interface IAuthorizationEvaluator
    {
        Task<AuthorizationResult> Evaluate(
            ClaimsPrincipal principal,
            object userContext,
            IDictionary<string, object> arguments,
            IEnumerable<string> requiredPolicies);
    }

    public class AuthorizationEvaluator : IAuthorizationEvaluator
    {
        private readonly AuthorizationSettings _settings;

        public AuthorizationEvaluator(AuthorizationSettings settings)
        {
            _settings = settings;
        }

        public async Task<AuthorizationResult> Evaluate(
            ClaimsPrincipal principal,
            object userContext,
            IDictionary<string, object> inputVariables,
            IEnumerable<string> requiredPolicies)
        {
            var context = new AuthorizationContext
            {
                User = principal ?? new ClaimsPrincipal(new ClaimsIdentity()),
                UserContext = userContext,
                InputVariables = inputVariables
            };

            var tasks = new List<Task>();

            requiredPolicies?.ToList()
                .Apply(requiredPolicy =>
                {
                    var authorizationPolicy = _settings.GetPolicy(requiredPolicy);
                    if (authorizationPolicy == null)
                    {
                        context.ReportError($"Required policy '{requiredPolicy}' is not present.");
                    }
                    else
                    {
                        authorizationPolicy.Requirements.Apply(r =>
                        {
                            var task = r.Authorize(context);
                            tasks.Add(task);
                        });
                    }
                });

            await Task.WhenAll(tasks.ToArray());

            return !context.HasErrors
                ? AuthorizationResult.Success()
                : AuthorizationResult.Fail(context.Errors);
        }
    }
}
