using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace GraphQL.Authorization
{
    public class AuthorizationContext
    {
        private List<string> _errors;

        public ClaimsPrincipal User { get; set; }

        public IDictionary<string, object> UserContext { get; set; }

        public Inputs InputVariables { get; set; }

        public IEnumerable<string> Errors => _errors ?? Enumerable.Empty<string>();

        public bool HasErrors => _errors?.Any() == true;

        /// <summary>
        /// Called by <see cref="IAuthorizationRequirement.Authorize(AuthorizationContext)"/> if the requirement is not met.
        /// </summary>
        /// <param name="error">Error message.</param>
        public void ReportError(string error)
        {
            if (_errors == null)
                _errors = new List<string>();

            _errors.Add(error);
        }
    }
}
