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
    public class ClaimAuthorizationRequirement : IAuthorizationRequirement
    {
        private readonly string _claimType;
        private readonly IEnumerable<string> _displayValues;
        private readonly IEnumerable<string> _allowedValues;

        /// <summary>
        /// Creates a new instance of <see cref="ClaimAuthorizationRequirement"/> with
        /// the specified claim type.
        /// </summary>
        public ClaimAuthorizationRequirement(string claimType)
            : this(claimType, (IEnumerable<string>)null, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ClaimAuthorizationRequirement"/> with
        /// the specified claim type and optional list of claim values, which, if present,
        /// the claim must match.
        /// </summary>
        public ClaimAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues)
            : this(claimType, allowedValues, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ClaimAuthorizationRequirement"/> with
        /// the specified claim type and optional list of claim values, which, if present,
        /// the claim must match.
        /// </summary>
        public ClaimAuthorizationRequirement(string claimType, params string[] allowedValues)
            : this(claimType, allowedValues, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="ClaimAuthorizationRequirement"/> with
        /// the specified claim type and optional list of claim values, which, if present,
        /// the claim must match. Additional <paramref name="displayValues"/> argument
        /// specifies the set of displayed claim values that will be used to generate an
        /// error message if the requirement is not met.
        /// </summary>
        public ClaimAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues, IEnumerable<string> displayValues)
        {
            _claimType = claimType ?? throw new ArgumentNullException(nameof(claimType));
            _allowedValues = allowedValues ?? Enumerable.Empty<string>();
            _displayValues = displayValues;
        }

        /// <inheritdoc />
        public Task Authorize(AuthorizationContext context)
        {
            bool found = false;

            if (context.User != null)
            {
                if (_allowedValues == null || !_allowedValues.Any())
                {
                    found = context.User.Claims.Any(
                        claim => string.Equals(claim.Type, _claimType, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    found = context.User.Claims.Any(
                        claim => string.Equals(claim.Type, _claimType, StringComparison.OrdinalIgnoreCase)
                             && _allowedValues.Contains(claim.Value, StringComparer.Ordinal));
                }
            }

            if (!found)
            {
                if (_allowedValues != null && _allowedValues.Any())
                {
                    string values = string.Join(", ", _displayValues ?? _allowedValues);
                    context.ReportError($"Required claim '{_claimType}' with any value of '{values}' is not present.");
                }
                else
                {
                    context.ReportError($"Required claim '{_claimType}' is not present.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
