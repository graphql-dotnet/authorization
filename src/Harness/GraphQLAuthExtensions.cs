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
    public static class GraphQLAuthExtensions
    {
        /// <summary>
        /// Adds all necessary classes into provided DI container.
        /// </summary>
        /// <param name="services">An instance of the DI container.</param>
        /// <param name="configure">Delegate to configure authorization settings.</param>
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
        /// Adds all necessary classes into provided DI container.
        /// </summary>
        /// <param name="services">An instance of the DI container.</param>
        /// <param name="configure">Delegate to configure authorization settings.</param>
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
