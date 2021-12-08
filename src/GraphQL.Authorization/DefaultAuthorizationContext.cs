using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Default implementation of <see cref="IAuthorizationContext"/>.
    /// </summary>
    public class DefaultAuthorizationContext : IAuthorizationContext
    {
        private bool _failCalled;
        private bool _succeedCalled;

        /// <summary>
        /// Creates a new instance of <see cref="DefaultAuthorizationContext"/>.
        /// </summary>
        /// <param name="policy">A checked policy for the current authorization processing.</param>
        /// <param name="user">A <see cref="ClaimsPrincipal"/> representing the current user.</param>
        public DefaultAuthorizationContext(IAuthorizationPolicy policy, ClaimsPrincipal? user)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            User = user;
            PendingRequirements = new HashSet<IAuthorizationRequirement>(policy.Requirements);
        }

        /// <inheritdoc />
        public IAuthorizationPolicy Policy { get; }

        /// <inheritdoc />
        public ClaimsPrincipal? User { get; }

        /// <inheritdoc />
        public IDictionary<string, object>? UserContext { get; set; }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, object>? Inputs { get; set; }

        /// <inheritdoc />
        public virtual IEnumerable<IAuthorizationRequirement> PendingRequirements { get; }

        /// <inheritdoc />
        public virtual bool HasFailed => _failCalled;

        /// <inheritdoc />
        public virtual bool HasSucceeded => !_failCalled && _succeedCalled && !PendingRequirements.Any();

        /// <inheritdoc />
        public virtual void Fail() => _failCalled = true;

        /// <inheritdoc />
        public virtual void Succeed(IAuthorizationRequirement requirement)
        {
            _succeedCalled = true;
            ((HashSet<IAuthorizationRequirement>)PendingRequirements).Remove(requirement);
        }
    }
}
