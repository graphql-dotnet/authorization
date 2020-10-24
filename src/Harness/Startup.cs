using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;

using GraphQL.Authorization;
using GraphQL.Types;
using GraphQL.Server;

namespace Harness
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<ISchema>(s =>
            {
                var definitions = @"
                  type User {
                    id: ID
                    name: String
                  }

                  type Query {
                    viewer: User
                    users: [User]
                  }
                ";
                var schema = Schema.For(
                    definitions,
                    _ =>
                    {
                        _.Types.Include<Query>();
                    });
                schema.FindType("User").AuthorizeWith("AdminPolicy");
                return schema;
            });

            // extension method defined in this project
            services.AddGraphQLAuth((_, s) =>
            {
                _.AddPolicy("AdminPolicy", p => p.RequireClaim("role", "Admin"));
            });

            // claims principal must look something like this to allow access
            // var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("role", "Admin") }));

            services.AddGraphQL()
                .AddSystemTextJson()
                .AddUserContextBuilder(context => new GraphQLUserContext { User = context.User });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseGraphQL<ISchema>("/graphql");
            app.UseGraphiQLServer();
        }
    }
}
