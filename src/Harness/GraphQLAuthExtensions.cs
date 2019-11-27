using GraphQL.Authorization;
using GraphQL.Server;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Harness
{
    public static class GraphQLAuthExtensions
    {
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
    }
}
