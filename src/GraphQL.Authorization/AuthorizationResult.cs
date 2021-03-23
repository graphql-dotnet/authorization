namespace GraphQL.Authorization
{
    /// <summary>
    /// Represents the result of an authorization evaluation.
    /// </summary>
    public class AuthorizationResult
    {
        // allocation optimization for green path
        private static readonly AuthorizationResult _success = new AuthorizationResult { Succeeded = true };

        private AuthorizationResult() { }

        /// <summary>
        /// Is the authorization result successful?
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Contains information about why authorization failed.
        /// </summary>
        public AuthorizationFailure? Failure { get; private set; }

        /// <summary>
        /// Creates successful authorization result.
        /// </summary>
        public static AuthorizationResult Success() => _success;

        /// <summary>
        /// Creates a failed authorization result.
        /// </summary>
        /// <param name="failure">Contains information about why authorization failed.</param>
        public static AuthorizationResult Failed(AuthorizationFailure failure) => new AuthorizationResult { Failure = failure };

        /// <summary>
        /// Creates a failed authorization result.
        /// </summary>
        /// <returns>The <see cref="AuthorizationResult"/>.</returns>
        public static AuthorizationResult Failed() => new AuthorizationResult { Failure = AuthorizationFailure.ExplicitFail() };
    }
}
