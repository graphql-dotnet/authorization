using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Authorization settings are represented by a set of named policies, each of which has a set of authorization requirements.
    /// </summary>
    public class AuthorizationSettings
    {
        private readonly IDictionary<string, IAuthorizationPolicy> _policies = new Dictionary<string, IAuthorizationPolicy>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<IAuthorizationPolicy> Policies => _policies.Values;

        public IEnumerable<IAuthorizationPolicy> GetPolicies(IEnumerable<string> policies)
        {
            List<IAuthorizationPolicy> found = null;

            policies?.Apply(name =>
            {
                var policy = GetPolicy(name);
                if (policy != null)
                {
                    if (found == null)
                        found = new List<IAuthorizationPolicy>();

                    found.Add(policy);
                }
            });

            return found ?? Enumerable.Empty<IAuthorizationPolicy>();
        }

        public IAuthorizationPolicy GetPolicy(string name) => _policies.TryGetValue(name, out var policy) ? policy : null;

        /// <summary>
        /// Adds a policy with the specified name. If a policy with that name already exists then it will be replaced.
        /// </summary>
        /// <param name="name">Policy name.</param>
        /// <param name="policy"></param>
        public void AddPolicy(string name, IAuthorizationPolicy policy) => _policies[name] = policy;

        /// <summary>
        /// Adds a policy built from <see cref="AuthorizationPolicyBuilder"/> with the specified name.
        /// </summary>
        /// <param name="name">Policy name.</param>
        /// <param name="configure">Delegate to configure provided policy builder.</param>
        public void AddPolicy(string name, Action<AuthorizationPolicyBuilder> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var builder = new AuthorizationPolicyBuilder();
            configure(builder);

            _policies[name] = builder.Build();
        }
    }
}
