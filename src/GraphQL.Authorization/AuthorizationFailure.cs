using System;
using System.Collections.Generic;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Encapsulates a failure result of <see cref="IAuthorizationService.AuthorizeAsync(IAuthorizationContext)"/>.
    /// </summary>
    public class AuthorizationFailure
    {
        private AuthorizationFailure() { }

        /// <summary>
        /// Failure was due to <see cref="IAuthorizationContext.Fail"/> being called.
        /// </summary>
        public bool FailCalled { get; private set; }

        /// <summary>
        /// Failure was due to these requirements not being met via <see cref="IAuthorizationContext.Succeed(IAuthorizationRequirement)"/>.
        /// </summary>
        public IEnumerable<IAuthorizationRequirement> FailedRequirements { get; private set; } = Array.Empty<IAuthorizationRequirement>();

        /// <summary>
        /// Return a failure due to <see cref="IAuthorizationContext.Fail"/> being called.
        /// </summary>
        /// <returns>The failure.</returns>
        public static AuthorizationFailure ExplicitFail() => new AuthorizationFailure { FailCalled = true };

        /// <summary>
        /// Return a failure due to some requirements not being met via <see cref="IAuthorizationContext.Succeed(IAuthorizationRequirement)"/>.
        /// </summary>
        /// <param name="failed">The requirements that were not met.</param>
        /// <returns>The failure.</returns>
        public static AuthorizationFailure Failed(IEnumerable<IAuthorizationRequirement> failed) => new AuthorizationFailure { FailedRequirements = failed };
    }
}
