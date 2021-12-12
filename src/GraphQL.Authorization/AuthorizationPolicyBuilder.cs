using System;
using System.Collections.Generic;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Configures and then builds authorization policy from various authorization requirements.
    /// Provides fluent API.
    /// </summary>
    public class AuthorizationPolicyBuilder
    {
        private readonly List<IAuthorizationRequirement> _requirements = new();

        /// <summary>
        /// Build authorization policy.
        /// </summary>
        /// <returns>Created policy.</returns>
        public AuthorizationPolicy Build() => new(_requirements);

        /// <summary>
        /// Adds <see cref="ClaimAuthorizationRequirement"/> with the specified claim type.
        /// </summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <returns>Reference to the same builder.</returns>
        public AuthorizationPolicyBuilder RequireClaim(string claimType)
        {
            _requirements.Add(new ClaimAuthorizationRequirement(claimType));
            return this;
        }

        /// <summary>
        /// Adds <see cref="ClaimAuthorizationRequirement"/> with the specified claim type and allowed values.
        /// </summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="allowedValues">Allowed values for this claim.</param>
        /// <returns>Reference to the same builder.</returns>
        public AuthorizationPolicyBuilder RequireClaim(string claimType, params string[] allowedValues)
        {
            _requirements.Add(new ClaimAuthorizationRequirement(claimType, allowedValues));
            return this;
        }

        /// <summary>
        /// Adds <see cref="ClaimAuthorizationRequirement"/> with the specified claim type, allowed values and display values.
        /// </summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="allowedValues">Allowed values for this claim.</param>
        /// <param name="displayValues">
        /// Display values for this claim. If no allowed claims are found, display values will be used to generate
        /// an error message for <see cref="AuthorizationContext.Errors"/>.
        /// </param>
        /// <returns>Reference to the same builder.</returns>
        public AuthorizationPolicyBuilder RequireClaim(string claimType, IEnumerable<string>? allowedValues, IEnumerable<string>? displayValues)
        {
            _requirements.Add(new ClaimAuthorizationRequirement(claimType, allowedValues, displayValues));
            return this;
        }

        /// <summary>
        /// Adds <see cref="AuthenticatedUserRequirement"/>.
        /// </summary>
        /// <returns>Reference to the same builder.</returns>
        public AuthorizationPolicyBuilder RequireAuthenticatedUser()
        {
            _requirements.Add(AuthenticatedUserRequirement.Instance);
            return this;
        }

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
