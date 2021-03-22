using System;
using System.Collections.Generic;
using GraphQL.Authorization;

namespace Harness
{
    public class Query
    {
        [GraphQLAuthorize(Policy = "AdminPolicy")]
        public User Viewer() => new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" };

        public List<User> Users() => new List<User> { new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" } };
    }

    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
