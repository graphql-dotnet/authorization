using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    public class ClaimAuthorizationRequirement : IAuthorizationRequirement
    {
        private readonly string _claimType;
        private readonly IEnumerable<string> _displayValues;
        private readonly IEnumerable<string> _allowedValues;

        public ClaimAuthorizationRequirement(string claimType)
            : this(claimType, (IEnumerable<string>)null, null)
        {
        }

        public ClaimAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues)
            : this(claimType, allowedValues, null)
        {
        }

        public ClaimAuthorizationRequirement(string claimType, params string[] allowedValues)
            : this(claimType, allowedValues, null)
        {
        }

        public ClaimAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues, IEnumerable<string> displayValues)
        {
            _claimType = claimType ?? throw new ArgumentNullException(nameof(claimType));
            _allowedValues = allowedValues ?? Enumerable.Empty<string>();
            _displayValues = displayValues;
        }

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
