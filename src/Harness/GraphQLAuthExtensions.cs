using System;
using GraphQL.Authorization;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Harness
{
    /// <summary>
    /// Extension methods to add GraphQL authorization into DI container.
    /// </summary>
    public static class GraphQLAuthExtensions // TODO: remove soon
    {
        /// <summary>
        /// Adds all necessary classes into provided <paramref name="services"/>
        /// and provides a delegate to configure authorization settings.
        /// </summary>
        public static void AddGraphQLAuth(this IServiceCollection services, Action<AuthorizationSettings, IServiceProvider> configure)
        {
            services.TryAddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
            services.AddTransient<IValidationRule, AuthorizationValidationRule>();

            services.TryAddTransient(s =>
            {
                var authSettings = new AuthorizationSettings();
                configure(authSettings, s);
                return authSettings;
            });
        }

        /// <summary>
        /// Adds all necessary classes into provided <paramref name="services"/>
        /// and provides a delegate to configure authorization settings.
        /// </summary>
        public static void AddGraphQLAuth(this IServiceCollection services, Action<AuthorizationSettings> configure)
        {
            services.TryAddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
            services.AddTransient<IValidationRule, AuthorizationValidationRule>();

            services.TryAddTransient(s =>
            {
                var authSettings = new AuthorizationSettings();
                configure(authSettings);
                return authSettings;
            });
        }
    }
}
