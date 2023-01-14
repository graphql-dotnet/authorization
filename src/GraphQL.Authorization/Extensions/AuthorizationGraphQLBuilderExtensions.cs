using GraphQL.Authorization;
using GraphQL.DI;

namespace GraphQL;

/// <summary>
/// Authorization extension methods for <see cref="IGraphQLBuilder"/>.
/// </summary>
public static class AuthorizationGraphQLBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="AuthorizationEvaluator"/> and <see cref="AuthorizationValidationRule"/> within the
    /// dependency injection framework and configures the validation rule to be added to the list of validation rules
    /// within <see cref="ExecutionOptions.ValidationRules"/> and <see cref="ExecutionOptions.CachedDocumentValidationRules"/>
    /// upon document execution. Configures authorization settings with the specified configuration delegate.
    /// </summary>
    public static IGraphQLBuilder AddAuthorization(this IGraphQLBuilder builder, Action<AuthorizationSettings, IServiceProvider> configure)
    {
        builder.Services.TryRegister<IAuthorizationEvaluator, AuthorizationEvaluator>(ServiceLifetime.Singleton);
        builder.AddValidationRule<AuthorizationValidationRule>(true);
        builder.Services.Configure(configure);
        return builder;
    }

    /// <inheritdoc cref="AddAuthorization(IGraphQLBuilder, Action{AuthorizationSettings, IServiceProvider})"/>
    public static IGraphQLBuilder AddAuthorization(this IGraphQLBuilder builder, Action<AuthorizationSettings> configure)
    {
        builder.Services.TryRegister<IAuthorizationEvaluator, AuthorizationEvaluator>(ServiceLifetime.Singleton);
        builder.AddValidationRule<AuthorizationValidationRule>(true);
        builder.Services.Configure(configure);
        return builder;
    }
}
