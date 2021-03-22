using System.Collections.Generic;
using System.Security.Claims;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Provides context information for <see cref="IAuthorizationRequirement"/>.
    /// </summary>
    public class AuthorizationContext
    {
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        /// Current user.
        /// </summary>
        public ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Arbitrary user defined context represented as dictionary.
        /// </summary>
        public IDictionary<string, object> UserContext { get; set; }

        /// <summary>
        /// Represents a readonly dictionary of variable inputs to an executed document.
        /// </summary>
        public IReadOnlyDictionary<string, object> Inputs { get; set; }

        /// <summary>
        /// Returns a set of authorization errors.
        /// </summary>
        public IEnumerable<string> Errors => _errors;

        /// <summary>
        /// Returns whether there are any errors.
        /// </summary>
        public bool HasErrors => _errors.Count > 0;

        /// <summary>
        /// Reports an error during evaluation of policy requirement.
        /// </summary>
        /// <param name="error">Error message.</param>
        public void ReportError(string error) => _errors.Add(error);
    }
}
