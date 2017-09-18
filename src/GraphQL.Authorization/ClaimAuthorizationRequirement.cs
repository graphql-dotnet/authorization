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
            : this(claimType, null, null)
        {
        }

        public ClaimAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues)
            : this(claimType, allowedValues, null)
        {
        }

        public ClaimAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues, IEnumerable<string> displayValues)
        {
            _claimType = claimType;
            _allowedValues = allowedValues ?? new List<string>();
            _displayValues = displayValues;
        }

        public Task Authorize(AuthorizationContext context)
        {
            var found = false;
            if (_allowedValues == null || !_allowedValues.Any())
            {
                found = context.User.Claims.Any(
                    c => string.Equals(c.Type, _claimType, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                found = context.User.Claims.Any(
                    c => string.Equals(c.Type, _claimType, StringComparison.OrdinalIgnoreCase)
                         && _allowedValues.Contains(c.Value, StringComparer.Ordinal));
            }

            if (!found)
            {
                if (_allowedValues != null && _allowedValues.Any())
                {
                    var values = string.Join(", ", _displayValues ?? _allowedValues);
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