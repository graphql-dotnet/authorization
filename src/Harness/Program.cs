using System.Security.Claims;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.TryAddSingleton<ISchema>(_ =>
{
    const string definitions = """
        type User {
          id: ID
          name: String
        }

        type Query {
          viewer: User
          users: [User]
          guest: String
        }
        """;
    var schema = Schema.For(definitions, builder => builder.Types.Include<Query>());
    schema.AllTypes["User"]!.AuthorizeWithPolicy("AdminPolicy");
    return schema;
});

// Claims principal must look something like this to allow access.
// GraphQLUserContext.User alternates below for demonstration purposes.
int counter = 0;
var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("role", "Admin") }));

builder.Services.AddGraphQL(builder => builder
    .AddSystemTextJson()
    .ConfigureExecutionOptions(opt => opt.Root = new Query())
    .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true)
    .AddAuthorization(settings => settings.AddPolicy("AdminPolicy", p => p.RequireClaim("role", "Admin")))
    .AddUserContextBuilder(context => new GraphQLUserContext { User = counter++ % 2 == 0 ? context.User : user }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseGraphQL();
app.UseGraphQLGraphiQL();
app.UseGraphQLPlayground(options: new GraphQL.Server.Ui.Playground.PlaygroundOptions { SchemaPollingEnabled = false });

app.Run();
