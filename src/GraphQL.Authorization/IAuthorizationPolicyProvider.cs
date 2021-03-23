namespace GraphQL.Authorization
{
    /// <summary>
    /// A type which can provide a <see cref="AuthorizationPolicy"/>.
    /// </summary>
    public interface IAuthorizationPolicyProvider
    {
        /// <summary>
        /// Gets a <see cref="AuthorizationPolicy"/> from the given <paramref name="policyName"/>.
        /// </summary>
        /// <param name="policyName">The policy name to retrieve.</param>
        /// <returns>The named <see cref="AuthorizationPolicy"/>.</returns>
        IAuthorizationPolicy GetPolicy(string policyName);
    }
}
