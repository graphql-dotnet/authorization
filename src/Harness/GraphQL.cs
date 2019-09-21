using System;
using System.Collections.Generic;

namespace Harness
{
    public class Query
    {
        // [GraphQLAuthorize(Policy = "AdminPolicy")]
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
