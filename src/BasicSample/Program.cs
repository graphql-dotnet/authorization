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
    class Program
    {
        static async Task Main(string[] args)
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
