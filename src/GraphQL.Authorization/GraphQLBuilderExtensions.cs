using System;
using GraphQL.DI;

namespace GraphQL.Authorization
{
    /// <inheritdoc cref="GraphQL.GraphQLBuilderExtensions"/>
    public static class GraphQLBuilderExtensions
    {
        /// <summary>
        /// Registers <see cref="AuthorizationEvaluator"/> and <see cref="AuthorizationValidationRule"/> within the
        /// dependency injection framework and configures the validation rule to be added to the list of validation rules
        /// within <see cref="ExecutionOptions.ValidationRules"/> and <see cref="ExecutionOptions.CachedDocumentValidationRules"/>
        /// upon document execution. Configures authorization settings with the specified configuration delegate.
        /// </summary>
        public static void AddAuthorization(this IGraphQLBuilder builder, Action<AuthorizationSettings, IServiceProvider> configure)
        {
            builder.TryRegister<IAuthorizationEvaluator, AuthorizationEvaluator>(ServiceLifetime.Singleton);
            builder.AddValidationRule<AuthorizationValidationRule>(true);
            builder.Configure(configure);
        }

        /// <inheritdoc cref="AddAuthorization(IGraphQLBuilder, Action{AuthorizationSettings, IServiceProvider})"/>
        public static void AddAuthorization(this IGraphQLBuilder builder, Action<AuthorizationSettings> configure)
        {
            builder.TryRegister<IAuthorizationEvaluator, AuthorizationEvaluator>(ServiceLifetime.Singleton);
            builder.AddValidationRule<AuthorizationValidationRule>(true);
            builder.Configure(configure);
        }
    }
}
