using System;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Implements an <see cref="IAuthorizationRequirement"/> that calls the specified delegate.
    /// </summary>
    public class DelegatedRequirement : IAuthorizationRequirement
    {
        private readonly Func<IAuthorizationContext, Task> _action;

        /// <summary>
        /// Creates a new instance of <see cref="DelegatedRequirement"/> with
        /// the specified delegate.
        /// </summary>
        public DelegatedRequirement(Func<IAuthorizationContext, Task> action)
        {
            _action = action;
        }

        /// <inheritdoc />
        public Task Authorize(IAuthorizationContext context) => _action(context);
    }
}
