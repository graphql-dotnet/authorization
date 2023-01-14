namespace GraphQL.Authorization;

/// <summary>
/// Authorization settings are represented by a set of named policies
/// where each policy has a set of authorization requirements.
/// </summary>
public class AuthorizationSettings
{
    private readonly IDictionary<string, IAuthorizationPolicy> _policies = new Dictionary<string, IAuthorizationPolicy>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns all policies.
    /// </summary>
    public IEnumerable<IAuthorizationPolicy> Policies => _policies.Values;

    /// <summary>
    /// Returns policies with the specified names.
    /// </summary>
    /// <param name="policies">A set of policies names.</param>
    /// <returns>Policies with matched names.</returns>
    public IEnumerable<IAuthorizationPolicy> GetPolicies(IEnumerable<string> policies)
    {
        List<IAuthorizationPolicy>? found = null;

        if (policies != null)
        {
            foreach (string name in policies)
            {
                var policy = GetPolicy(name);
                if (policy != null)
                    (found ??= new()).Add(policy);
            }
        }

        return found ?? Enumerable.Empty<IAuthorizationPolicy>();
    }

    /// <summary>
    /// Returns one policy with the specified name.
    /// </summary>
    /// <param name="name">Name of the required policy.</param>
    /// <returns>Required policy if exists, otherwise <see langword="null"/>.</returns>
    public IAuthorizationPolicy? GetPolicy(string name) => _policies.TryGetValue(name, out var policy) ? policy : null;

    /// <summary>
    /// Adds a policy with the specified name. If a policy with that name already exists then it will be replaced.
    /// </summary>
    /// <param name="name">Policy name.</param>
    /// <param name="policy">Policy to add.</param>
    public void AddPolicy(string name, IAuthorizationPolicy policy) => _policies[name] = policy;

    /// <summary>
    /// Adds a policy built from <see cref="AuthorizationPolicyBuilder"/> with the specified name.
    /// </summary>
    /// <param name="name">Policy name.</param>
    /// <param name="configure">Delegate to configure provided policy builder.</param>
    public void AddPolicy(string name, Action<AuthorizationPolicyBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var builder = new AuthorizationPolicyBuilder();
        configure(builder);

        _policies[name] = builder.Build();
    }
}
