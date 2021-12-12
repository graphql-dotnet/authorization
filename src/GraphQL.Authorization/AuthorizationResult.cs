using System.Collections.Generic;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Represents the result of an authorization evaluation.
    /// </summary>
    public class AuthorizationResult
    {
        // allocation optimization for green path
        private static readonly AuthorizationResult _success = new() { Succeeded = true };

        /// <summary>
        /// Is the authorization result successful?
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Returns a set of authorization errors if the authorization result is unsuccessful.
        /// </summary>
        public IEnumerable<string>? Errors { get; private set; }

        /// <summary>
        /// Creates successful authorization result.
        /// </summary>
        /// <returns>Instance of <see cref="AuthorizationResult"/>.</returns>
        public static AuthorizationResult Success() => _success;

        /// <summary>
        /// Creates unsuccessful authorization result
        /// </summary>
        /// <param name="errors">A set of authorization errors.</param>
        /// <returns>Instance of <see cref="AuthorizationResult"/>.</returns>
        public static AuthorizationResult Fail(IEnumerable<string> errors) => new() { Errors = errors };
    }
}
