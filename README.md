# GraphQL Authorization

[![Build Status](https://ci.appveyor.com/api/projects/status/github/graphql-dotnet/authorization?branch=master&svg=true)](https://ci.appveyor.com/project/graphql-dotnet-ci/authorization)
[![NuGet](https://img.shields.io/nuget/v/GraphQL.Authorization.svg)](https://www.nuget.org/packages/GraphQL.Authorization/)
[![Join the chat at https://gitter.im/graphql-dotnet/graphql-dotnet](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/graphql-dotnet/graphql-dotnet?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A toolset for authorizing access to graph types for [GraphQL .NET](https://github.com/graphql-dotnet/graphql-dotnet).

# Usage

* Register the authorization classes in your container (`IAuthorizationEvaluator`, `AuthorizationSettings`, and the `AuthorizationValidationRule`).
* Provide a `UserContext` class that implements `IProvideClaimsPrincipal`.
* Add policies to the `AuthorizationSettings`.
* Apply a policy to a GraphType or Field (which implement `IProvideMetadata`) using `AuthorizeWith(string policy)`.
* The `AuthorizationValidationRule` will run and verify the policies based on the registered policies.
* You can write your own `IAuthorizationRequirement`.
* Use `GraphQLAuthorize` attribute if using Schema + Handler syntax.

# Examples

```csharp
public static void AddGraphQLAuth(this IServiceCollection services)
{
    services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.TryAddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
    services.AddTransient<IValidationRule, AuthorizationValidationRule>();

    services.TryAddSingleton(s =>
    {
        var authSettings = new AuthorizationSettings();

        authSettings.AddPolicy("AdminPolicy", _ => _.RequireClaim("role", "Admin"));

        return authSettings;
    });
}


public static void AddGraphQLAuth(this IApplicationBuilder app)
{
    var settings = new GraphQLSettings
    {
        BuildUserContext = ctx =>
        {
            var userContext = new GraphQLUserContext
            {
                User = ctx.User
            };

            return Task.FromResult(userContext);
        }
    };

    var rules = app.ApplicationServices.GetServices<IValidationRule>();
    settings.ValidationRules.AddRange(rules);

    app.UseMiddleware<GraphQLMiddleware>(settings);
}

public class GraphQLUserContext : IProvideClaimsPrincipal
{
    public ClaimsPrincipal User { get; set; }
}

public class GraphQLSettings
{
    public Func<HttpContext, Task<object>> BuildUserContext { get; set; }
    public object Root { get; set; }
    public IList<IValidationRule> ValidationRules { get; } = new List<IValidationRule>();
}
```

Type syntax - use `AuthorizeWith`.

```csharp
public class MyType : ObjectGraphType
{
    public MyType()
    {
        this.AuthorizeWith("AdminPolicy");
        Field<StringGraphType>("name").AuthorizeWith("SomePolicy");
    }
}
```

Schema + Handler syntax - use `GraphQLAuthorize` attribute.

```csharp
[GraphQLAuthorize(Policy = "MyPolicy")]
public class MutationType
{
    [GraphQLAuthorize(Policy = "AnotherPolicy")]
    public async Task<string> CreateSomething(MyInput input)
    {
        return Guid.NewGuid().ToString();
    }
}
```
