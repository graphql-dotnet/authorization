# GraphQL Authorization

![License](https://img.shields.io/github/license/graphql-dotnet/authorization)

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
It provides a [validation rule](src/GraphQL.Authorization/AuthorizationValidationRule.cs) that checks all of the
Graph Types in the given GraphQL operation (query/mutation/subscription) to see if they have authorization policies
applied to them and evaluates these policies if any.

Provides the following packages:

| Package               | Downloads                                                                                                               | NuGet Latest                                                                                                             |
|-----------------------|-------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------|
| GraphQL.Authorization | [![Nuget](https://img.shields.io/nuget/dt/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization) | [![Nuget](https://img.shields.io/nuget/v/GraphQL.Authorization)](https://www.nuget.org/packages/GraphQL.Authorization)   |

You can get all preview versions from [GitHub Packages](https://github.com/orgs/graphql-dotnet/packages?repo_name=authorization).
Note that GitHub requires authentication to consume the feed. See [here](https://docs.github.com/en/free-pro-team@latest/packages/publishing-and-managing-packages/about-github-packages#authenticating-to-github-packages).

## Note for ASP.NET Core users

If you came here in search for GraphQL authorization for the ASP.NET Core applications,
then it makes sense to look into the [server](https://github.com/graphql-dotnet/server) project
and its [GraphQL.Server.Authorization.AspNetCore](https://www.nuget.org/packages/GraphQL.Server.Authorization.AspNetCore)
package. Although you will be able to integrate GraphQL authorization with the help of classes
from the current repository, the _GraphQL.Server.Authorization.AspNetCore_ package is much better
adapted to work within the ASP.NET Core applications.

## Usage

1. Register the necessary authorization classes in your DI container:
   - `IValidationRule/AuthorizationValidationRule`
   - `IAuthorizationService/DefaultAuthorizationService`
   - `IClaimsPrincipalAccessor/DefaultClaimsPrincipalAccessor`
   - `IAuthorizationPolicyProvider/DefaultAuthorizationPolicyProvider`
2. If you use `DefaultClaimsPrincipalAccessor` then provide a custom `UserContext`
class that implements `IProvideClaimsPrincipal`.
3. Add policies to the `AuthorizationSettings`.
4. Apply a policy to a `GraphType` or `FieldType` (both implement `IProvideMetadata`):
   - using `AuthorizeWith(string policy)` extension method
   - or with `GraphQLAuthorize` attribute if using Schema + Handler syntax.
5. The `AuthorizationValidationRule` will run and verify the policies based on the
registered policies.
6. You can write your own `IAuthorizationRequirement` and an extension method to add
this requirement to `AuthorizationPolicyBuilder`.

## Examples

#### Examples in this repository

1. Fully functional basic [Console sample](src/BasicSample/Program.cs).

2. Fully functional [ASP.NET Core sample](src/Harness/Program.cs).

#### Add authorization policy [GraphType first syntax]

Use `AuthorizeWith` extension method on `IGraphType` or `IFieldType`.

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

#### Add authorization policy [Schema first syntax]

Use `GraphQLAuthorize` attribute on type, method or property.

```csharp
[GraphQLAuthorize(Policy = "MyPolicy")]
public class MutationType
{
    [GraphQLAuthorize(Policy = "AnotherPolicy")]
    public async Task<string> CreateSomething(MyInput input)
    {
        return await SomeMethodAsync(input);
    }

    [GraphQLAuthorize(Policy = "SuperPolicy")]
    public string SomeProperty => Guid.NewGuid().ToString();
}
```

#### Custom authorization requirement

You can add your own requirements to the authorization framework to extend it.
Create your own `IAuthorizationRequirement` class and add that requirement to your policy.

```csharp
public class OnlyMondayRequirement : IAuthorizationRequirement
{
    public Task Authorize(IAuthorizationContext context)
    {
        if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            context.Succeed(this);
    }
}

public static class MyAuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequireMonday(this AuthorizationPolicyBuilder builder)
    {
        builder.AddRequirement(new OnlyMondayRequirement());
        return builder;
    }
}

public static void ConfigureAuthorizationServices(ServiceCollection services)
{
    services
        .AddSingleton<IValidationRule, AuthorizationValidationRule>()
        .AddSingleton<IAuthorizationService, DefaultAuthorizationService>()
        .AddSingleton<IClaimsPrincipalAccessor, DefaultClaimsPrincipalAccessor>()
        .AddSingleton<IAuthorizationPolicyProvider>(provider =>
        {
            var authSettings = new AuthorizationSettings();
            authSettings.AddPolicy("MyPolicy", b => b.RequireMonday());
            return new DefaultAuthorizationPolicyProvider(authSettings);
        })
}
```

#### How to change error messages

Authorization requirement (`IAuthorizationRequirement`) only checks the compliance of
the current execution state to some criteria. If the requirement is satisfied, then
it is marked as 'passed' and the next requirement is checked. If all requirements are
satisfied, then the validation rule returns a successful result. Otherwise for each
unsatisfied requirement, the validation rule will add an authorization error in the
`ValidationContext`. The text of this error may not suit you, especially if you write
your own authorization requirements because by default you will see only _You are not
authorized to run this query_ text which does not contain any details about your
requirement. This is done for security reasons but you can override the default behavior.

**Option 1.** If you are satisfied with the existing error messages and you only want
to add error message for your own authorization requirement, then inherit your authorization
requirement from `IAuthorizationRequirementWithErrorMessage` interface.

```csharp
public class OnlyMondayRequirement : IAuthorizationRequirementWithErrorMessage
{
    public Task Authorize(IAuthorizationContext context)
    {
        if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            context.Succeed(this);
    }

    public string ErrorMessage => "Access is allowed only on Mondays.";
}
```

**Option 2.** If you want to get full control over the whole error message for authorization
process then inherit from `AuthorizationValidationRule` and override `AddValidationError`
or `BuildErrorMessage` methods. Then register `CustomAuthorizationValidationRule` class
instead of `AuthorizationValidationRule` class in your DI container.

**Option 3.** Another way to get full control over the whole error message sent to client
is to implement `IErrorInfoProvider` interface. This is one of the interfaces from the
main GraphQL.NET repository. For convenience you may use the `ErrorInfoProvider` base class. 

```csharp
public class CustomErrorInfoProvider : ErrorInfoProvider
{
    public override ErrorInfo GetInfo(ExecutionError executionError)
    {
        var info = base.GetInfo(executionError);
        info.Message = executionError switch
        {
            AuthorizationError authorizationError => "You shall not pass!",
            _ => info.Message,
        };
        return info;
    }
}
```

Then register `CustomErrorInfoProvider` in your DI container.

```csharp
services.AddSingleton<IErrorInfoProvider, CustomErrorInfoProvider>();
```

## Known Issues

* It is currently not possible to add a policy to Input objects using Schema first approach.
