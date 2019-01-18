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
            Settings.AddPolicy("ClassPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldPassRule(_=>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        {"Admin", "true"}
                    });
            });
        }

        [Fact]
        public void class_policy_fail()
        {
            Settings.AddPolicy("ClassPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldFailRule(_=>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema();
            });
        }

        [Fact]
        public void field_policy_success()
        {
            Settings.AddPolicy("FieldPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldPassRule(_=>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        {"Admin", "true"}
                    });
            });
        }

        [Fact]
        public void field_policy_fail()
        {
            Settings.AddPolicy("FieldPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldFailRule(_=>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema();
            });
        }

        [Fact]
        public void nested_type_policy_success()
        {
            Settings.AddPolicy("PostPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldPassRule(_=>
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
            Settings.AddPolicy("PostPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldFailRule(_=>
            {
                _.Query = @"query { post }";
                _.Schema = NestedSchema();
            });
        }

        [Fact]
        public void nested_type_list_policy_fail()
        {
            Settings.AddPolicy("PostPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldFailRule(_=>
            {
                _.Query = @"query { posts }";
                _.Schema = NestedSchema();
            });
        }

        [Fact]
        public void nested_type_list_non_null_policy_fail()
        {
            Settings.AddPolicy("PostPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldFailRule(_=>
            {
                _.Query = @"query { postsNonNull }";
                _.Schema = NestedSchema();
            });
        }

        [Fact]
        public void passes_with_claim_on_input_type()
        {
            Settings.AddPolicy("FieldPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldPassRule(_=>
            {
                _.Query = @"query { author(input: { name: ""Quinn"" }) }";
                _.Schema = TypedSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        {"Admin", "true"}
                    });
            });
        }

        [Fact]
        public void fails_on_missing_claim_on_input_type()
        {
            Settings.AddPolicy("FieldPolicy", _ =>
            {
                _.RequireClaim("admin");
            });

            ShouldFailRule(_=>
            {
                _.Query = @"query { author(input: { name: ""Quinn"" }) }";
                _.Schema = TypedSchema();
            });
        }

        private ISchema BasicSchema()
        {
            var defs = @"
                type Query {
                    post(id: ID!): String
                }
            ";

            return Schema.For(defs, _ =>
            {
                _.Types.Include<BasicQueryWithAttributes>();
            });
        }

        [GraphQLMetadata("Query")]
        [GraphQLAuthorize(Policy = "ClassPolicy")]
        public class BasicQueryWithAttributes
        {
            [GraphQLAuthorize(Policy = "FieldPolicy")]
            public string Post(string id)
            {
                return "";
            }
        }

        private ISchema NestedSchema()
        {
            var defs = @"
                type Query {
                    post(id: ID!): Post
                    posts: [Post]
                    postsNonNull: [Post!]!
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

            public IEnumerable<Post> Posts()
            {
                return null;
            }

            public IEnumerable<Post> PostsNonNull()
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
            public string Name { get; set;}
        }

        private ISchema TypedSchema()
        {
            var query = new ObjectGraphType();
            query.Field<StringGraphType>(
                "author",
                arguments: new QueryArguments(new QueryArgument<AuthorInputType> { Name = "input" }),
                resolve: context => "testing"
            );
            return new Schema { Query = query };
        }

        public class AuthorInputType : InputObjectGraphType<Author>
        {
            public AuthorInputType()
            {
                Field(x => x.Name).AuthorizeWith("FieldPolicy");
            }
        }
    }
}
