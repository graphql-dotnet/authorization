# GraphQL Authorization

[![Join the chat at https://gitter.im/graphql-dotnet/graphql-dotnet](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/graphql-dotnet/graphql-dotnet?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Run code tests](https://github.com/graphql-dotnet/authorization/actions/workflows/test.yml/badge.svg)](https://github.com/graphql-dotnet/authorization/actions/workflows/test.yml)
[![Build artifacts](https://github.com/graphql-dotnet/authorization/actions/workflows/build.yml/badge.svg)](https://github.com/graphql-dotnet/authorization/actions/workflows/build.yml)
[![Publish release](https://github.com/graphql-dotnet/authorization/actions/workflows/publish.yml/badge.svg)](https://github.com/graphql-dotnet/authorization/actions/workflows/publish.yml)
[![CodeQL analysis](https://github.com/graphql-dotnet/authorization/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/graphql-dotnet/authorization/actions/workflows/codeql-analysis.yml)

[![codecov](https://codecov.io/gh/graphql-dotnet/authorization/branch/master/graph/badge.svg?token=TODO)](https://codecov.io/gh/graphql-dotnet/authorization)
[![Total alerts](https://img.shields.io/lgtm/alerts/g/graphql-dotnet/authorization.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/graphql-dotnet/authorization/alerts/)
[![Language grade: C#](https://img.shields.io/lgtm/grade/csharp/g/graphql-dotnet/authorization.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/graphql-dotnet/authorization/context:csharp)

![Activity](https://img.shields.io/github/commit-activity/w/graphql-dotnet/authorization)
![Activity](https://img.shields.io/github/commit-activity/m/graphql-dotnet/authorization)
![Activity](https://img.shields.io/github/commit-activity/y/graphql-dotnet/authorization)

![Size](https://img.shields.io/github/repo-size/graphql-dotnet/authorization)

A toolset for authorizing access to graph types for [GraphQL.NET](https://github.com/graphql-dotnet/graphql-dotnet).

Provides the following packages:

| Package               | Downloads                                                                                                               | NuGet Latest                                                                                                             |
|-----------------------|-------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------|
| GraphQL.Authorization | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization) | [![Nuget](https://img.shields.io/nuget/v/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization)   |

You can get all preview versions from [GitHub Packages](https://github.com/orgs/graphql-dotnet/packages?repo_name=authorization).
Note that GitHub requires authentication to consume the feed. See [here](https://docs.github.com/en/free-pro-team@latest/packages/publishing-and-managing-packages/about-github-packages#authenticating-to-github-packages).

# Usage

* Register the authorization classes in your DI container (`IAuthorizationEvaluator`, `AuthorizationSettings`, and the `AuthorizationValidationRule`).
* Provide a `UserContext` class that implements `IProvideClaimsPrincipal`.
* Add policies to the `AuthorizationSettings`.
* Apply a policy to a GraphType or Field (both implement `IProvideMetadata`):
  - using `AuthorizeWith(string policy)` extension method
  - or with `GraphQLAuthorize` attribute if using Schema + Handler syntax.
* The `AuthorizationValidationRule` will run and verify the policies based on the registered policies.
* You can write your own `IAuthorizationRequirement`.

# Examples

1. Fully functional [basic sample](src/BasicSample/Program.cs).
Register the authorization classes in your container:

```csharp
public static IGraphQLBuilder AddGraphQLAuth(this IGraphQLBuilder builder, Action<AuthorizationSettings, IServiceProvider> configure)
{
    if (builder == null)
        throw new ArgumentNullException(nameof(builder));

    builder.Services.AddHttpContextAccessor();
    builder.Services.TryAddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
    builder.Services.AddTransient<IValidationRule, AuthorizationValidationRule>();

    builder.Services.TryAddTransient(provider =>
    {
        var authSettings = new AuthorizationSettings();
        configure(authSettings, provider);
        return authSettings;
    });

    return builder;
}

public static IGraphQLBuilder AddGraphQLAuth(this IGraphQLBuilder builder, Action<AuthorizationSettings> configure)
{
    if (configure == null)
        throw new ArgumentNullException(nameof(configure));

    return builder.AddGraphQLAuth((settings, _) => configure(settings));
}
```

Provide a `UserContext` class that implements `IProvideClaimsPrincipal` and add policies to the `AuthorizationSettings`:

```csharp
public class GraphQLUserContext : IProvideClaimsPrincipal
{
    public ClaimsPrincipal User { get; set; }
}

// AddGraphQL is an extension method from the GraphQL.Server.Core package and it is aware of all registered validation rules
// see https://github.com/graphql-dotnet/server/blob/develop/src/Core/ServiceCollectionExtensions.cs
services.AddGraphQL(options =>
{
    options.ExposeExceptions = true;
    options.EnableMetrics = false;
})
.AddUserContextBuilder(context => new GraphQLUserContext { User = context.User })
.AddGraphQLAuth(settings =>
{
    settings.AddPolicy("AdminPolicy", options => options.RequireClaim("role", "Admin"));
}
```

Register your schema and append GraphQL middleware in the HTTP request pipeline:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ISchema, YourSchema>();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    // UseGraphQL is an extension method from the GraphQL.Server.Transports.AspNetCore package
    // see https://github.com/graphql-dotnet/server/blob/develop/src/Transports.AspNetCore/ApplicationBuilderExtensions.cs
    app.UseGraphQL<ISchema>();
}
```

GraphType first syntax - use `AuthorizeWith` extension method on GraphType or Field.

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

3. Schema first syntax - use `GraphQLAuthorize` attribute on type or method.

```c#
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

# Known Issues

* It is currently not possible to add a policy to Input objects using Schema first approach.
