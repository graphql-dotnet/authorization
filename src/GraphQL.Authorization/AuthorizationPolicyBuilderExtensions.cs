using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Extension methods for <see cref="AuthorizationPolicyBuilder"/>.
    /// </summary>
    public static class AuthorizationPolicyBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="ClaimsAuthorizationRequirement"/> with the specified claim type.
        /// </summary>
        /// <param name="builder">Authorization policy builder to add requirement to.</param>
        /// <param name="claimType">Type of the claim.</param>
        /// <returns>Reference to the same builder.</returns>
        public static AuthorizationPolicyBuilder RequireClaim(this AuthorizationPolicyBuilder builder, string claimType)
            => builder.AddRequirement(new ClaimsAuthorizationRequirement(claimType));

        /// <summary>
        /// Adds <see cref="ClaimsAuthorizationRequirement"/> with the specified claim type and allowed values.
        /// </summary>
        /// <param name="builder">Authorization policy builder to add requirement to.</param>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="allowedValues">Allowed values for this claim.</param>
        /// <returns>Reference to the same builder.</returns>
        public static AuthorizationPolicyBuilder RequireClaim(this AuthorizationPolicyBuilder builder, string claimType, params string[] allowedValues)
            => builder.AddRequirement(new ClaimsAuthorizationRequirement(claimType, allowedValues));

        /// <summary>
        /// Adds <see cref="ClaimsAuthorizationRequirement"/> with the specified claim type, allowed values and display values.
        /// </summary>
        /// <param name="builder">Authorization policy builder to add requirement to.</param>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="allowedValues">Allowed values for this claim.</param>
        /// <param name="displayValues">
        /// Display values for this claim. If no allowed claims are found, display values should be used to generate
        /// an error message if the requirement is not met.
        /// </param>
        /// <returns>Reference to the same builder.</returns>
        public static AuthorizationPolicyBuilder RequireClaim(this AuthorizationPolicyBuilder builder, string claimType, IEnumerable<string> allowedValues, IEnumerable<string> displayValues)
            => builder.AddRequirement(new ClaimsAuthorizationRequirement(claimType, allowedValues, displayValues));

        /// <summary>
        /// Adds <see cref="AuthenticatedUserRequirement"/>.
        /// </summary>
        /// <param name="builder">Authorization policy builder to add requirement to.</param>
        /// <returns>Reference to the same builder.</returns>
        public static AuthorizationPolicyBuilder RequireAuthenticatedUser(this AuthorizationPolicyBuilder builder)
            => builder.AddRequirement(AuthenticatedUserRequirement.Instance);

        /// <summary>
        /// Adds <see cref="DelegatedRequirement"/> with the specified delegate.
        /// </summary>
        /// <param name="builder">Authorization policy builder to add requirement to.</param>
        /// <param name="action">Delegate to execute.</param>
        /// <returns>Reference to the same builder.</returns>
        public static AuthorizationPolicyBuilder Require(this AuthorizationPolicyBuilder builder, Func<IAuthorizationContext, Task> action)
            => builder.AddRequirement(new DelegatedRequirement(action));

        /// <summary>
        /// Adds <see cref="DelegatedRequirement"/> with the specified delegate.
        /// </summary>
        /// <param name="builder">Authorization policy builder to add requirement to.</param>
        /// <param name="action">Delegate to execute.</param>
        /// <returns>Reference to the same builder.</returns>
        public static AuthorizationPolicyBuilder Require(this AuthorizationPolicyBuilder builder, Action<IAuthorizationContext> action)
            => builder.AddRequirement(new DelegatedRequirement(context =>
            {
                action(context);
                return Task.CompletedTask;
            }));
    }
}
