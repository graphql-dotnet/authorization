using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace BasicSample
{
    internal class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "main")]
        private static async Task Main()
        {
            using var serviceProvider = new ServiceCollection()
                .AddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>()
                .AddTransient<IValidationRule, AuthorizationValidationRule>()
                .AddTransient(s =>
                {
                    var authSettings = new AuthorizationSettings();
                    authSettings.AddPolicy("AdminPolicy", p => p.RequireClaim("role", "Admin"));
                    return authSettings;
                })
                .BuildServiceProvider();

            string definitions = @"
                type User {
                    id: ID
                    name: String
                }

                type Query {
                    viewer: User
                    users: [User]
                }
            ";
            var schema = Schema.For(definitions, builder => builder.Types.Include<Query>());

            // remove claims to see the failure
            var authorizedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("role", "Admin") }));

            string json = await schema.ExecuteAsync(_ =>
            {
                _.Query = "{ viewer { id name } }";
                _.ValidationRules = serviceProvider
                    .GetServices<IValidationRule>()
                    .Concat(DocumentValidator.CoreRules);
                _.RequestServices = serviceProvider;
                _.UserContext = new GraphQLUserContext { User = authorizedUser };
            });

            Console.WriteLine(json);
        }
    }

    /// <summary>
    /// Custom context class that implements <see cref="IProvideClaimsPrincipal"/>.
    /// </summary>
    public class GraphQLUserContext : Dictionary<string, object>, IProvideClaimsPrincipal
    {
        /// <inheritdoc />
        public ClaimsPrincipal User { get; set; }
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
        public User Viewer() => new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" };

        /// <summary>
        /// Resolver for 'Query.users' field.
        /// </summary>
        public List<User> Users() => new List<User> { new User { Id = Guid.NewGuid().ToString(), Name = "Quinn" } };
    }

    /// <summary>
    /// CLR type to map to the 'User' graph type.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Resolver for 'User.id' field. Just a simple property.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Resolver for 'User.name' field. Just a simple property.
        /// </summary>
        public string Name { get; set; }
    }
}
