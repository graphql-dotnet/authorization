using GraphQL.Authorization;
using GraphQL.Server;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Types;
using GraphQL.Validation;
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
                  }
                ";
                return Schema.For(
                    definitions,
                    _ =>
                    {
                        _.Types.Include<Query>();
                    });
            });

            // Extension method defined in this project
            services.AddTransient<IValidationRule, AuthorizationValidationRule>();
            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("AdminPolicy", p => p.RequireClaim("role", "Admin"));
                });

            services.AddGraphQL(options =>
            {
                options.ExposeExceptions = true;
            }).AddUserContextBuilder(context => new GraphQLUserContext { User = context.User });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            var validationRules = app.ApplicationServices.GetServices<IValidationRule>();

            app.UseGraphQL<ISchema>("/graphql");
            app.UseGraphiQLServer(new GraphiQLOptions());

            app.UseMvc();
        }
    }
}
