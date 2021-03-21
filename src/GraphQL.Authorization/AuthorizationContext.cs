using System.Collections.Generic;
using System.Security.Claims;

namespace GraphQL.Authorization
{
    public class AuthorizationContext
    {
        private readonly List<string> _errors = new List<string>();

        public ClaimsPrincipal User { get; set; }

        public object UserContext { get; set; }

        public IDictionary<string, object> InputVariables { get; set; }

        public IEnumerable<string> Errors => _errors;

        public bool HasErrors => _errors.Count > 0;

        public void ReportError(string error)
        {
            _errors.Add(error);
        }
    }
}
