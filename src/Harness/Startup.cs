using GraphQL.Authorization;
using GraphQL.Server;
using GraphQL.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                    builder =>
                    {
                        builder.Types.Include<Query>();
                    });
                schema.FindType("User").AuthorizeWith("AdminPolicy");
                return schema;
            });

            // extension method defined in this project
            services.AddGraphQLAuth(settings =>
            {
                settings.AddPolicy("AdminPolicy", builder => builder.RequireClaim("role", "Admin"));
            });

            services.AddGraphQL(options =>
            {
                options.ExposeExceptions = true;
                options.EnableMetrics = false;
            }).AddUserContextBuilder(context => new GraphQLUserContext { User = context.User });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage()
               .UseGraphQL<ISchema>()
               .UseGraphiQLServer();
        }
    }
}
