using GraphQL.Types;
using Shouldly;
using System.Linq;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class AuthorizationSchemaBuilderTests
    {
        [Fact]
        public void can_set_policy_from_authorize_attribute()
        {
            string defs = @"
                type Query {
                    post(id: ID!): String
                }
            ";

            var schema = Schema.For(defs, builder => builder.Types.Include<QueryWithAttributes>());

            schema.Initialize();

            var query = schema.FindType("Query") as IObjectGraphType;
            query.RequiresAuthorization().ShouldBeTrue();
            query.GetPolicies().Single().ShouldBe("ClassPolicy");

            var field = query.Fields.Single(x => x.Name == "post");
            field.RequiresAuthorization().ShouldBeTrue();
            field.GetPolicies().Single().ShouldBe("FieldPolicy");
        }

        [GraphQLMetadata("Query")]
        [GraphQLAuthorize(Policy = "ClassPolicy")]
        public class QueryWithAttributes
        {
            [GraphQLAuthorize(Policy = "FieldPolicy")]
            public string Post(string id) => "";
        }
    }
}
