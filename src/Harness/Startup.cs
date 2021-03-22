using GraphQL;
using GraphQL.Server;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

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
            services.TryAddSingleton(s =>
            {
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
                schema.AllTypes["User"].AuthorizeWith("AdminPolicy");
                return schema;
            });

            // extension method defined in this project
            services.AddGraphQLAuth((settings, provider) => settings.AddPolicy("AdminPolicy", p => p.RequireClaim("role", "Admin")));

            // claims principal must look something like this to allow access
            // var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("role", "Admin") }));

            services.AddGraphQL()
                .AddSystemTextJson()
                .AddUserContextBuilder(context => new GraphQLUserContext { User = context.User });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseGraphQL<ISchema>();
            app.UseGraphQLGraphiQL();
            app.UseGraphQLPlayground();
        }
    }
}
