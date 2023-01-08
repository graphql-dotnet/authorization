using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
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
// var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("role", "Admin") }));

builder.Services.AddGraphQL(builder => builder
    .AddSystemTextJson()
    .ConfigureExecutionOptions(opt =>
    {
        opt.ThrowOnUnhandledException = true;
        opt.Root = new Query();
        // opt.User = user; // User property has already been initialized. Uncomment to play with ClaimsPrincipal.
    })
    .AddErrorInfoProvider(opt => opt.ExposeExceptionDetails = true)
    .AddAuthorization(settings => settings.AddPolicy("AdminPolicy", p => p.RequireClaim("role", "Admin"))));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseGraphQL();
app.UseGraphQLGraphiQL();
app.UseGraphQLPlayground(options: new GraphQL.Server.Ui.Playground.PlaygroundOptions { SchemaPollingEnabled = false });

app.Run();
