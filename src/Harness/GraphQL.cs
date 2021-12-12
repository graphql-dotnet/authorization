using System;
using System.Collections.Generic;
using System.Security.Claims;
using GraphQL;
using GraphQL.Authorization;

namespace Harness
{
    /// <summary>
    /// Custom context class that implements <see cref="IProvideClaimsPrincipal"/>.
    /// </summary>
    public class GraphQLUserContext : Dictionary<string, object?>, IProvideClaimsPrincipal
    {
        /// <inheritdoc />
        public ClaimsPrincipal? User { get; set; }
    }

    /// <summary>
    /// CLR type to map to the 'Query' graph type.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Resolver for 'Query.viewer' field.
        /// </summary>
        [GraphQLAuthorize("AdminPolicy")]
        public User Viewer() => new() { Id = Guid.NewGuid().ToString(), Name = "Quinn" };

        /// <summary>
        /// Resolver for 'Query.users' field.
        /// </summary>
        public List<User> Users() => new() { new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" } };
    }

    /// <summary>
    /// CLR type to map to the 'User' graph type.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Resolver for 'User.id' field. Just a simple property.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Resolver for 'User.name' field. Just a simple property.
        /// </summary>
        public string? Name { get; set; }
    }
}
