using System;
using System.Collections.Generic;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Configures and then builds authorization policy from various authorization requirements.
    /// </summary>
    public class AuthorizationPolicyBuilder
    {
        private readonly List<IAuthorizationRequirement> _requirements = new List<IAuthorizationRequirement>();

        /// <summary>
        /// Build authorization policy.
        /// </summary>
        /// <returns>Created policy.</returns>
        public AuthorizationPolicy Build() => new AuthorizationPolicy(_requirements);

        /// <summary>
        /// Adds specified authorization requirement.
        /// </summary>
        /// <param name="requirement">Authorization requirement.</param>
        /// <returns>Reference to the same builder.</returns>
        public AuthorizationPolicyBuilder AddRequirement(IAuthorizationRequirement requirement)
        {
            _requirements.Add(requirement ?? throw new ArgumentNullException(nameof(requirement)));
            return this;
        }
    }
}
