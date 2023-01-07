using System.Security.Claims;
using GraphQL;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection().AddGraphQL(builder => builder
    .AddAuthorization(settings => settings.AddPolicy("AdminPolicy", p => p.RequireClaim("role", "Admin"))));

using var serviceProvider = services.BuildServiceProvider();

const string definitions = """
    type User {
        id: ID
        name: String
    }

    type Query {
        viewer: User
        users: [User]
    }
    """;
var schema = Schema.For(definitions, builder => builder.Types.Include<Query>());

// Claims principal must look something like this to allow access.
// GraphQLUserContext.User alternates below for demonstration purposes.
int counter = 0;
var authorizedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("role", "Admin") }));
var nonAuthorizedUser = new ClaimsPrincipal(new ClaimsIdentity());

while (true)
{
    string json = await schema.ExecuteAsync(options =>
    {
        options.Query = "{ viewer { id name } }";
        options.Root = new Query();
        options.ValidationRules = DocumentValidator.CoreRules.Concat(serviceProvider.GetServices<IValidationRule>());
        options.RequestServices = serviceProvider;
        options.UserContext = new GraphQLUserContext { User = counter++ % 2 == 0 ? authorizedUser : nonAuthorizedUser };
    }).ConfigureAwait(false);

    Console.WriteLine(json);
    Console.WriteLine();
    Console.WriteLine("Press ENTER to continue");
    Console.ReadLine();
}
