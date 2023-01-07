namespace GraphQL.Authorization;

/// <summary>
/// Policy is a named set of <see cref="IAuthorizationRequirement"/>.
/// </summary>
public interface IAuthorizationPolicy
{
    /// <summary>
    /// Returns all requirements of this policy.
    /// </summary>
    IEnumerable<IAuthorizationRequirement> Requirements { get; }
}
