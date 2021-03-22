using System.Collections.Generic;
using System.Security.Claims;

namespace GraphQL.Authorization.Tests
{
    internal class GraphQLUserContext : Dictionary<string, object>, IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }
    }
}
