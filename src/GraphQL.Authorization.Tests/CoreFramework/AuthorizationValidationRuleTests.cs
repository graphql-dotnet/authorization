#if (!NET46)
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class AuthorizationValidationRuleTests : ValidationTestBase
    {
        [Fact]
        public void class_policy_success()
        {
            ConfigureAuthorizationOptions(
                options =>
                {
                    options.AddPolicy("ClassPolicy", x => x.RequireClaim("admin"));
                });

            ShouldPassRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema<BasicQueryWithAttributesAndClassPolicy>();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        {"Admin", "true"}
                    });
            });
        }

        [Fact]
        public void class_policy_fail()
        {
            ConfigureAuthorizationOptions(
                options =>
                {
                    options.AddPolicy("ClassPolicy", x => x.RequireClaim("admin"));
                });

            ShouldFailRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema<BasicQueryWithAttributesAndClassPolicy>();
            });
        }

        [Fact]
        public void field_policy_success()
        {
            ConfigureAuthorizationOptions(
                options =>
                {
                    options.AddPolicy("FieldPolicy", x => x.RequireClaim("admin"));
                });

            ShouldPassRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema<BasicQueryWithAttributesAndFieldPolicy>();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        {"Admin", "true"}
                    });
            });
        }

        [Fact]
        public void field_policy_fail()
        {
            ConfigureAuthorizationOptions(
                options =>
                {
                    options.AddPolicy("FieldPolicy", x => x.RequireClaim("admin"));
                });

            ShouldFailRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema<BasicQueryWithAttributesAndFieldPolicy>();
            });
        }

        [Fact]
        public void nested_type_policy_success()
        {
            ConfigureAuthorizationOptions(
                options =>
                {
                    options.AddPolicy("PostPolicy", x => x.RequireClaim("admin"));
                });

            ShouldPassRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = NestedSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        {"Admin", "true"}
                    });
            });
        }

        [Fact]
        public void nested_type_policy_fail()
        {
            ConfigureAuthorizationOptions(
                options =>
                {
                    options.AddPolicy("PostPolicy", x => x.RequireClaim("admin"));
                });

            ShouldFailRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = NestedSchema();
            });
        }

        private ISchema BasicSchema<T>()
        {
            string defs = @"
                type Query {
                    post(id: ID!): String
                }
            ";

            return Schema.For(defs, _ =>
            {
                _.Types.Include<T>();
            });
        }

        [GraphQLMetadata("Query")]
        [GraphQLAuthorize(Policy = "ClassPolicy")]
        public class BasicQueryWithAttributesAndClassPolicy
        {
            public string Post(string id)
            {
                return "";
            }
        }

        [GraphQLMetadata("Query")]
        public class BasicQueryWithAttributesAndFieldPolicy
        {
            [GraphQLAuthorize(Policy = "FieldPolicy")]
            public string Post(string id)
            {
                return "";
            }
        }

        private ISchema NestedSchema()
        {
            string defs = @"
                type Query {
                    post(id: ID!): Post
                }

                type Post {
                    id: ID!
                }
            ";

            return Schema.For(defs, _ =>
            {
                _.Types.Include<NestedQueryWithAttributes>();
                _.Types.Include<Post>();
            });
        }

        [GraphQLMetadata("Query")]
        public class NestedQueryWithAttributes
        {
            public Post Post(string id)
            {
                return null;
            }
        }

        [GraphQLAuthorize(Policy = "PostPolicy")]
        public class Post
        {
            public string Id { get; set; }
        }

        public class Author
        {
            public string Name { get; set; }
        }
    }
}
#endif
