using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Implements an <see cref="IAuthorizationRequirement"/> which requires an instance of the specified
    /// claim type, and, if allowed values are specified, the claim value must be any of the allowed values.
    /// </summary>
    public class ClaimsAuthorizationRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Creates a new instance of <see cref="ClaimsAuthorizationRequirement"/> with
        /// the specified claim type.
        /// </summary>
        public ClaimsAuthorizationRequirement(string claimType)
            : this(claimType, (IEnumerable<string>)null, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ClaimsAuthorizationRequirement"/> with
        /// the specified claim type and optional list of claim values, which, if present,
        /// the claim must match.
        /// </summary>
        public ClaimsAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues)
            : this(claimType, allowedValues, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ClaimsAuthorizationRequirement"/> with
        /// the specified claim type and optional list of claim values, which, if present,
        /// the claim must match.
        /// </summary>
        public ClaimsAuthorizationRequirement(string claimType, params string[] allowedValues)
            : this(claimType, allowedValues, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ClaimsAuthorizationRequirement"/> with
        /// the specified claim type and optional list of claim values, which, if present,
        /// the claim must match. Additional <paramref name="displayValues"/> argument
        /// specifies the set of displayed claim values that should be used to generate an
        /// error message if the requirement is not met.
        /// </summary>
        public ClaimsAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues, IEnumerable<string> displayValues)
        {
            ClaimType = claimType ?? throw new ArgumentNullException(nameof(claimType));
            AllowedValues = allowedValues ?? Enumerable.Empty<string>();
            DisplayValues = displayValues;
        }

        /// <summary>
        /// Gets the claim type that must be present.
        /// </summary>
        public string ClaimType { get; }

        /// <summary>
        /// Gets the optional list of claim values, which, if present, the claim must match.
        /// </summary>
        public IEnumerable<string> AllowedValues { get; }

        /// <summary>
        /// Specifies the set of displayed claim values that should be used to generate an
        /// error message if the requirement is not met.
        /// </summary>
        public IEnumerable<string> DisplayValues { get; }

        /// <inheritdoc />
        public Task Authorize(IAuthorizationContext context)
        {
            bool found = false;

            if (context.User != null)
            {
                if (AllowedValues == null || !AllowedValues.Any())
                {
                    found = context.User.Claims.Any(
                        claim => string.Equals(claim.Type, ClaimType, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    found = context.User.Claims.Any(
                        claim => string.Equals(claim.Type, ClaimType, StringComparison.OrdinalIgnoreCase)
                             && AllowedValues.Contains(claim.Value, StringComparer.Ordinal));
                }
            }

            if (found)
                context.Succeed(this);

            return Task.CompletedTask;
        }
    }
}
