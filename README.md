# GraphQL Authorization

[![Join the chat at https://gitter.im/graphql-dotnet/graphql-dotnet](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/graphql-dotnet/graphql-dotnet?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build status](https://github.com/graphql-dotnet/authorization/workflows/Build%20artifacts/badge.svg)](https://github.com/graphql-dotnet/authorization/actions)
[![Build status](https://github.com/graphql-dotnet/authorization/workflows/Publish%20release/badge.svg)](https://github.com/graphql-dotnet/authorization/actions)

[![NuGet](https://img.shields.io/nuget/v/GraphQL.Authorization.svg)](https://www.nuget.org/packages/GraphQL.Authorization)
[![Nuget](https://img.shields.io/nuget/dt/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization)

![Activity](https://img.shields.io/github/commit-activity/w/graphql-dotnet/authorization)
![Activity](https://img.shields.io/github/commit-activity/m/graphql-dotnet/authorization)
![Activity](https://img.shields.io/github/commit-activity/y/graphql-dotnet/authorization)

![Size](https://img.shields.io/github/repo-size/graphql-dotnet/authorization)

A toolset for authorizing access to graph types for [GraphQL.NET](https://github.com/graphql-dotnet/graphql-dotnet).

# Usage

* Register the authorization classes in your DI container (`IAuthorizationEvaluator`, `AuthorizationSettings`, and the `AuthorizationValidationRule`).
* Provide a `UserContext` class that implements `IProvideClaimsPrincipal`.
* Add policies to the `AuthorizationSettings`.
* Apply a policy to a GraphType or Field (which implement `IProvideMetadata`) using `AuthorizeWith(string policy)`.
* Make sure the `AuthorizationValidationRule` is registered with your Schema (depending on your server implementation, you may only need to register it in your DI container)
* The `AuthorizationValidationRule` will run and verify the policies based on the registered policies.
* You can write your own `IAuthorizationRequirement`.
* Use `GraphQLAuthorize` attribute if using Schema First syntax.

# Examples

1. Fully functional [basic sample](src/BasicSample/Program.cs).

2. GraphType first syntax - use `AuthorizeWith`.

```c#
public class MyType : ObjectGraphType
{
    public MyType()
    {
        this.AuthorizeWith("AdminPolicy");
        Field<StringGraphType>("name").AuthorizeWith("SomePolicy");
    }
}
```

3. Schema first syntax - use `GraphQLAuthorize` attribute.

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
