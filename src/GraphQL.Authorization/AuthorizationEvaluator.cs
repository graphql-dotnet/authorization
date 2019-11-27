using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    public class AuthorizationEvaluator : IAuthorizationEvaluator
    {
        private readonly AuthorizationSettings _settings;

        public AuthorizationEvaluator(AuthorizationSettings settings)
        {
            _settings = settings;
        }

        public async Task<AuthorizationResult> Evaluate(
            ClaimsPrincipal principal,
            IDictionary<string, object> userContext,
            Dictionary<string, object> inputVariables,
            IEnumerable<string> requiredPolicies)
        {
            if (requiredPolicies == null)
                return AuthorizationResult.Success();

            var context = new AuthorizationContext
            {
                User = principal ?? new ClaimsPrincipal(new ClaimsIdentity()),
                UserContext = userContext,
                InputVariables = inputVariables
            };

            var tasks = new List<Task>();

            requiredPolicies.Apply(requiredPolicy =>
            {
                var authorizationPolicy = _settings.GetPolicy(requiredPolicy);
                if (authorizationPolicy == null)
                {
                    context.ReportError($"Required policy '{requiredPolicy}' is not present.");
                }
                else
                {
                    authorizationPolicy.Requirements.Apply(req => tasks.Add(req.Authorize(context)));
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return context.HasErrors
                ? AuthorizationResult.Fail(context.Errors)
                : AuthorizationResult.Success();
        }
    }
}
