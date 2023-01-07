# GraphQL Authorization

[![License](https://img.shields.io/github/license/graphql-dotnet/authorization)](LICENSE.md)
[![codecov](https://codecov.io/gh/graphql-dotnet/authorization/branch/master/graph/badge.svg?token=TODO)](https://codecov.io/gh/graphql-dotnet/authorization)
[![Nuget](https://img.shields.io/nuget/dt/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization)
[![Nuget](https://img.shields.io/nuget/v/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization)
[![GitHub Release Date](https://img.shields.io/github/release-date/graphql-dotnet/authorization?label=released)](https://github.com/graphql-dotnet/authorization/releases)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/graphql-dotnet/authorization/latest?label=new+commits)](https://github.com/graphql-dotnet/authorization/commits/master)
![Size](https://img.shields.io/github/repo-size/graphql-dotnet/authorization)

[![GitHub contributors](https://img.shields.io/github/contributors/graphql-dotnet/authorization)](https://github.com/graphql-dotnet/authorization/graphs/contributors)
![Activity](https://img.shields.io/github/commit-activity/w/graphql-dotnet/authorization)
![Activity](https://img.shields.io/github/commit-activity/m/graphql-dotnet/authorization)
![Activity](https://img.shields.io/github/commit-activity/y/graphql-dotnet/authorization)

A toolset for authorizing access to graph types for [GraphQL.NET](https://github.com/graphql-dotnet/graphql-dotnet).

Provides the following packages:

| Package               | Downloads                                                                                                               | NuGet Latest                                                                                                           |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| GraphQL.Authorization | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization) | [![Nuget](https://img.shields.io/nuget/v/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization) |

You can get all preview versions from [GitHub Packages](https://github.com/orgs/graphql-dotnet/packages?repo_name=authorization).
Note that GitHub requires authentication to consume the feed. See [here](https://docs.github.com/en/free-pro-team@latest/packages/publishing-and-managing-packages/about-github-packages#authenticating-to-github-packages).

# Usage

- Register the authorization classes in your DI container - call `AddAuthorization` on the provided `IGraphQLBuilder` inside `AddGraphQL` extension method.
- Provide a custom `UserContext` class that implements `IProvideClaimsPrincipal`.
- Add policies to the `AuthorizationSettings`.
- Apply a policy to a GraphType or Field - both implement `IProvideMetadata`:
  - using `AuthorizeWithPolicy(string policy)` extension method
  - or with `AuthorizeAttribute` attribute if using Schema + Handler syntax.
- The `AuthorizationValidationRule` will run and verify the policies based on the registered policies.
- You can write your own `IAuthorizationRequirement`.

# Examples

1. Fully functional basic [Console sample](src/BasicSample/Program.cs).

2. Fully functional [ASP.NET Core sample](src/Harness/Program.cs).

3. GraphType first syntax - use `AuthorizeWithPolicy` extension method on `IGraphType` or `IFieldType`.

```csharp
public class MyType : ObjectGraphType
{
    public MyType()
    {
        this.AuthorizeWithPolicy("AdminPolicy");
        Field<StringGraphType>("name").AuthorizeWithPolicy("SomePolicy");
    }
}
```

4. Schema first syntax - use `AuthorizeAttribute` attribute on type, method or property.

```csharp
[Authorize("MyPolicy")]
public class MutationType
{
    [Authorize("AnotherPolicy")]
    public async Task<string> CreateSomething(MyInput input)
    {
        return await SomeMethodAsync(input);
    }

    [Authorize("SuperPolicy")]
    public string SomeProperty => Guid.NewGuid().ToString();
}
```

# Known Issues

- It is currently not possible to add a policy to Input objects using Schema first approach.
