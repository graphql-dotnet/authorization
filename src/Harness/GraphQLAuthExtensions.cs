using GraphQL.Authorization;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Harness
{
    public static class GraphQLAuthExtensions
    {
        public static IServiceCollection AddGraphQLAuth(this IServiceCollection services, Action<AuthorizationSettings, IServiceProvider> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
            services.AddTransient<IValidationRule, AuthorizationValidationRule>();

            services.TryAddTransient(provider =>
            {
                var authSettings = new AuthorizationSettings();
                configure(authSettings, provider);
                return authSettings;
            });

            return services;
        }

        public static IServiceCollection AddGraphQLAuth(this IServiceCollection services, Action<AuthorizationSettings> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            return services.AddGraphQLAuth((settings, provider) => configure(settings));
        }
    }
}
