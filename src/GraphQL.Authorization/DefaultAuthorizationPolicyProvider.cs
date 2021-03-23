namespace GraphQL.Authorization
{
    /// <summary>
    /// Default implementation of <see cref="IAuthorizationPolicyProvider"/> that gets
    /// policies from the configured <see cref="AuthorizationSettings"/>.
    /// </summary>
    public class DefaultAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly AuthorizationSettings _settings;

        /// <summary>
        /// Creates an instance of <see cref="DefaultAuthorizationPolicyProvider"/> with the
        /// specified authorization settings.
        /// </summary>
        public DefaultAuthorizationPolicyProvider(AuthorizationSettings settings)
        {
            _settings = settings;
        }

        /// <inheritdoc />
        public IAuthorizationPolicy? GetPolicy(string policyName) => _settings.GetPolicy(policyName);
    }
}
