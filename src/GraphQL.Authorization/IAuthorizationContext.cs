using System.Collections.Generic;
using System.Security.Claims;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Provides context information for the current authorization processing.
    /// </summary>
    public interface IAuthorizationContext
    {
        /// <summary>
        /// A checked policy for the current authorization processing.
        /// </summary>
        IAuthorizationPolicy Policy { get; }

        /// <summary>
        /// Current user.
        /// </summary>
        ClaimsPrincipal? User { get; }

        /// <summary>
        /// Arbitrary user defined context represented as a dictionary.
        /// </summary>
        IDictionary<string, object>? UserContext { get; }

        /// <summary>
        /// Represents a readonly dictionary of variable inputs to an executed document.
        /// </summary>
        IReadOnlyDictionary<string, object>? Inputs { get; }

        /// <summary>
        /// Gets the requirements that have not yet been marked as succeeded.
        /// </summary>
        IEnumerable<IAuthorizationRequirement> PendingRequirements { get; }

        /// <summary>
        /// Flag indicating whether the current authorization processing has failed.
        /// </summary>
        bool HasFailed { get; }

        /// <summary>
        /// Flag indicating whether the current authorization processing has succeeded.
        /// </summary>
        bool HasSucceeded { get; }

        /// <summary>
        /// Called to indicate <see cref="HasSucceeded"/> will
        /// never return <see langword="true"/>, even if all requirements are met.
        /// </summary>
        void Fail();

        /// <summary>
        /// Called to mark the specified <paramref name="requirement"/> as being
        /// successfully evaluated.
        /// </summary>
        /// <param name="requirement">The requirement whose evaluation has succeeded.</param>
        void Succeed(IAuthorizationRequirement requirement);
    }
}
