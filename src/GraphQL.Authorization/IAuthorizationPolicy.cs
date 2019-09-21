using System.Collections.Generic;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Policy is a named set of <see cref="AuthorizationRequirement"/>.
    /// </summary>
    public interface IAuthorizationPolicy
    {
        IEnumerable<IAuthorizationRequirement> Requirements { get; }
    }
}
