namespace GraphQL.Authorization;

/// <summary>
/// Default implementation for <see cref="IAuthorizationPolicy"/>.
/// </summary>
public class AuthorizationPolicy : IAuthorizationPolicy
{
    private readonly List<IAuthorizationRequirement> _requirements = new();

    /// <summary>
    /// Creates a policy with a set of specified requirements.
    /// </summary>
    /// <param name="requirements">Specified requirements.</param>
    public AuthorizationPolicy(IEnumerable<IAuthorizationRequirement> requirements)
    {
        if (requirements != null)
        {
            _requirements.AddRange(requirements);
            _requirements.ForEach(req =>
            {
                if (req == null)
                    throw new ArgumentNullException(nameof(requirements), $"One of the ({_requirements.Count}) requirements is null");
            });
        }
    }

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> Requirements => _requirements;
}
