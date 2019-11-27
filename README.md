# GraphQL Authorization

[![Build Status](https://ci.appveyor.com/api/projects/status/github/graphql-dotnet/authorization?branch=master&svg=true)](https://ci.appveyor.com/project/graphql-dotnet-ci/authorization)
[![NuGet](https://img.shields.io/nuget/v/GraphQL.Authorization.svg)](https://www.nuget.org/packages/GraphQL.Authorization/)
[![Join the chat at https://gitter.im/graphql-dotnet/graphql-dotnet](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/graphql-dotnet/graphql-dotnet?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A toolset for authorizing access to graph types for [GraphQL .NET](https://github.com/graphql-dotnet/graphql-dotnet).

# Usage

* Register the authorization classes in your container (`IAuthorizationEvaluator`, `AuthorizationSettings`, and the `AuthorizationValidationRule`).
* Provide a `UserContext` class that implements `IProvideClaimsPrincipal`.
* Add policies to the `AuthorizationSettings`.
* Apply a policy to a GraphType or Field (both implement `IProvideMetadata`):
  - using `AuthorizeWith(string policy)` extension method
  - or with `GraphQLAuthorize` attribute if using Schema + Handler syntax.
* The `AuthorizationValidationRule` will run and verify the policies based on the registered policies.
* You can write your own `IAuthorizationRequirement`.

# Examples

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

Schema first syntax - use `GraphQLAuthorize` attribute on type or method.

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

# Known Issues

* It is currently not possible to add a policy to Input objects using Schema first approach.
