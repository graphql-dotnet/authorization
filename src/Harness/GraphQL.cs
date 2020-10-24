using System;
using System.Collections.Generic;
using System.Security.Claims;
using GraphQL.Authorization;

namespace Harness
{
    public class GraphQLUserContext : Dictionary<string, object>, IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }
    }

    public class Query
    {
        [GraphQLAuthorize(Policy = "AdminPolicy")]
        public User Viewer()
        {
            return new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" };
        }

        public List<User> Users()
        {
            return new List<User> { new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" } };
        }
    }

    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
