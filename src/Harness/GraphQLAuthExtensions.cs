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
        /// Adds all necessary classes into provided <paramref name="services"/>
        /// and provides a delegate to configure authorization settings.
        /// </summary>
        public static void AddGraphQLAuth(this IServiceCollection services, Action<AuthorizationSettings, IServiceProvider> configure)
        {
            services.TryAddSingleton<IValidationRule, AuthorizationValidationRule>();
            services.TryAddSingleton<IAuthorizationService, DefaultAuthorizationService>();
            services.TryAddSingleton<IClaimsPrincipalAccessor, DefaultClaimsPrincipalAccessor>();
            services.TryAddSingleton<IAuthorizationPolicyProvider>(provider =>
            {
                var authSettings = new AuthorizationSettings();
                configure(authSettings, provider);
                return new DefaultAuthorizationPolicyProvider(authSettings);
            });
        }

        /// <summary>
        /// Adds all necessary classes into provided <paramref name="services"/>
        /// and provides a delegate to configure authorization settings.
        /// </summary>
        public static void AddGraphQLAuth(this IServiceCollection services, Action<AuthorizationSettings> configure)
            => services.AddGraphQLAuth((settings, _) => configure(settings));
    }
}
