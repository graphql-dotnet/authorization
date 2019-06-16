using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Validation;
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
