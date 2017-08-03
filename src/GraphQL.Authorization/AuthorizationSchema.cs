using System;
using GraphQL.Types;

namespace GraphQL.Authorization
{
    public sealed class AuthorizationSchema
    {
        public static ISchema For(
            string typeDefinitions,
            Action<AuthorizationSchemaBuilder> configure = null)
        {
            var builder = new AuthorizationSchemaBuilder();
            configure?.Invoke(builder);
            return builder.Build(typeDefinitions);
        }
    }
}
