using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Default implementation of <see cref="IAuthorizationEvaluator"/>.
    /// </summary>
    public class AuthorizationEvaluator : IAuthorizationEvaluator
    {
        private readonly AuthorizationSettings _settings;

        /// <summary>
        /// Creates an instance of <see cref="AuthorizationEvaluator"/> with the
        /// specified authorization settings.
        /// </summary>
        public AuthorizationEvaluator(AuthorizationSettings settings)
        {
            _settings = settings;
        }

        /// <inheritdoc />
        public async Task<AuthorizationResult> Evaluate(
            ClaimsPrincipal principal,
            IDictionary<string, object> userContext,
            IReadOnlyDictionary<string, object> inputs,
            IEnumerable<string> requiredPolicies)
        {
            if (requiredPolicies == null)
                return AuthorizationResult.Success();

            var context = new AuthorizationContext
            {
                User = principal ?? new ClaimsPrincipal(new ClaimsIdentity()),
                UserContext = userContext,
                Inputs = inputs
            };

            var tasks = new List<Task>();

            foreach (string requiredPolicy in requiredPolicies)
            {
                var authorizationPolicy = _settings.GetPolicy(requiredPolicy);
                if (authorizationPolicy == null)
                {
                    context.ReportError($"Required policy '{requiredPolicy}' is not present.");
                }
                else
                {
                    foreach (var r in authorizationPolicy.Requirements)
                    {
                        var task = r.Authorize(context);
                        tasks.Add(task);
                    }
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return context.HasErrors
                ? AuthorizationResult.Fail(context.Errors)
                : AuthorizationResult.Success();
        }
    }
}
