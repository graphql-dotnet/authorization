using System;
using System.Collections.Generic;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Builder assembles policy from various authorization requirements.
    /// </summary>
    public class AuthorizationPolicyBuilder
    {
        private readonly List<IAuthorizationRequirement> _requirements = new List<IAuthorizationRequirement>();

        public AuthorizationPolicy Build() => new AuthorizationPolicy(_requirements);

        public AuthorizationPolicyBuilder RequireClaim(string claimType)
        {
            _requirements.Add(new ClaimAuthorizationRequirement(claimType));
            return this;
        }

        public AuthorizationPolicyBuilder RequireClaim(string claimType, params string[] allowedValues)
        {
            _requirements.Add(new ClaimAuthorizationRequirement(claimType, allowedValues));
            return this;
        }

        public AuthorizationPolicyBuilder RequireClaim(string claimType, IEnumerable<string> allowedValues, IEnumerable<string> displayValues)
        {
            _requirements.Add(new ClaimAuthorizationRequirement(claimType, allowedValues, displayValues));
            return this;
        }

        public AuthorizationPolicyBuilder RequireAuthenticatedUser()
        {
            _requirements.Add(new AuthenticatedUserRequirement());
            return this;
        }

        public AuthorizationPolicyBuilder AddRequirement(IAuthorizationRequirement requirement)
        {
            _requirements.Add(requirement ?? throw new ArgumentNullException(nameof(requirement)));
            return this;
        }
    }
}
