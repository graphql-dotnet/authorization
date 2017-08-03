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
            IEnumerable<string> requiredPolicies)
        {
            var context = new AuthorizationContext();
            context.User = principal ?? new ClaimsPrincipal(new ClaimsIdentity());
            context.UserContext = userContext;

            var authPolicies = _settings.GetPolicies(requiredPolicies).ToList();

            var tasks = new List<Task>();

            authPolicies.Apply(p =>
            {
                p.Requirements.Apply(r =>
                {
                    var task = r.Authorize(context);
                    tasks.Add(task);
                });
            });

            await Task.WhenAll(tasks.ToArray());

            return !context.HasErrors
                ? AuthorizationResult.Success()
                : AuthorizationResult.Fail(context.Errors);
        }
    }
}