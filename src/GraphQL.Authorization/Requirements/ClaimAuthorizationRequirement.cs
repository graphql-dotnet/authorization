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
        private readonly IEnumerable<string> _displayValues;

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
            ClaimType = claimType ?? throw new ArgumentNullException(nameof(claimType));
            AllowedValues = allowedValues ?? Enumerable.Empty<string>();
            _displayValues = displayValues;
        }

        /// <summary>
        ///  The specified claim type
        /// </summary>
        public string ClaimType { get; }

        /// <summary>
        /// The specified values of claim type.
        /// </summary>
        public IEnumerable<string> AllowedValues { get; }


        /// <inheritdoc />
        public Task Authorize(AuthorizationContext context)
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

            if (!found)
            {
                if (AllowedValues != null && AllowedValues.Any())
                {
                    string values = string.Join(", ", _displayValues ?? AllowedValues);
                    context.ReportError($"Required claim '{ClaimType}' with any value of '{values}' is not present.");
                }
                else
                {
                    context.ReportError($"Required claim '{ClaimType}' is not present.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
