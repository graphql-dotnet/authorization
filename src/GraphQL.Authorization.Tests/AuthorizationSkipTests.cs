using GraphQL.Types;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    /// <summary>
    /// Tests for <see cref="IntrospectionSkipCondition"/>.
    /// https://github.com/graphql-dotnet/authorization/issues/28
    /// </summary>
    public class AuthorizationSkipTests : ValidationTestBase
    {
        [Fact]
        public void passes_with_skip_condition()
        {
            Rule = new AuthorizationValidationRule(new AuthorizationEvaluator(Settings), new[] { new IntrospectionSkipCondition() });
            Settings.AddPolicy("AdminPolicy", _ => _.RequireClaim("admin"));

            ShouldPassRule(config =>
            {
                config.Query = QUERY;
                config.Schema = CreateSchema();
            });
        }

        [Fact]
        public void fails_without_skip_condition()
        {
            Settings.AddPolicy("AdminPolicy", _ => _.RequireClaim("admin"));

            ShouldFailRule(config =>
            {
                config.Query = QUERY;
                config.Schema = CreateSchema();
            });
        }

        [Fact]
        public void fails_with_skip_condition_and_extra_fields()
        {
            Rule = new AuthorizationValidationRule(new AuthorizationEvaluator(Settings), new[] { new IntrospectionSkipCondition() });
            Settings.AddPolicy("AdminPolicy", _ => _.RequireClaim("admin"));

            ShouldFailRule(config =>
            {
                config.Query = QUERY.Replace("...frag1", "...frag1 info");
                config.Schema = CreateSchema();
            });
        }

        private static ISchema CreateSchema() =>
            Schema.For("type Query { info: String! }", builder => builder.Types.Include<Query>());

        [GraphQLAuthorize("AdminPolicy")]
        public class Query
        {
            public string Info() => "OK";
        }

        private const string QUERY = @"
query
{
  __typename
  __type(name: ""__Schema"")
  {
    name
    description
  }
  x: __schema
  {
    queryType
    {
      name
    }
  }
  ...frag1
  ... on Query
  {
    inline: __typename
  }
}

fragment frag1 on Query
{
  s: __typename
}";
    }
}
