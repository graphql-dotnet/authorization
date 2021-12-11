using System.Linq;
using GraphQL.Types;
using Shouldly;
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

            var query = (IObjectGraphType)schema.AllTypes["Query"]!;
            query.RequiresAuthorization().ShouldBeTrue();
            query.GetPolicies()!.Single().ShouldBe("ClassPolicy");

            var field = query.Fields.Single(x => x.Name == "post");
            field.RequiresAuthorization().ShouldBeTrue();
            field.GetPolicies()!.Single().ShouldBe("FieldPolicy");
        }

        [GraphQLMetadata("Query")]
        [GraphQLAuthorize("ClassPolicy")]
        public class QueryWithAttributes
        {
            [GraphQLAuthorize("FieldPolicy")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "test")]
            public string Post(string id) => "";
        }
    }
}
