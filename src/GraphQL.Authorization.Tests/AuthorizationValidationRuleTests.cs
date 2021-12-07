using System.Collections.Generic;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class AuthorizationValidationRuleTests : ValidationTestBase
    {
        [Fact]
        public void class_policy_success()
        {
            Settings.AddPolicy("ClassPolicy", builder => builder.RequireClaim("admin"));
            Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

            ShouldPassRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        { "Admin", "true" }
                    });
            });
        }

        [Fact]
        public void class_policy_fail()
        {
            Settings.AddPolicy("ClassPolicy", builder => builder.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema();
            });
        }

        [Fact]
        public void field_policy_success()
        {
            Settings.AddPolicy("ClassPolicy", builder => builder.RequireClaim("admin"));
            Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

            ShouldPassRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        { "Admin", "true" }
                    });
            });
        }

        [Fact]
        public void field_policy_fail()
        {
            Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = BasicSchema();
            });
        }

        [Fact]
        public void nested_type_policy_success()
        {
            Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

            ShouldPassRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = NestedSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        { "Admin", "true" }
                    });
            });
        }

        [Fact]
        public void nested_type_policy_fail()
        {
            Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { post }";
                _.Schema = NestedSchema();
            });
        }

        [Fact]
        public void nested_type_list_policy_fail()
        {
            Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { posts }";
                _.Schema = NestedSchema();
            });
        }

        [Fact]
        public void nested_type_list_non_null_policy_fail()
        {
            Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { postsNonNull }";
                _.Schema = NestedSchema();
            });
        }

        [Fact]
        public void passes_with_claim_on_input_type()
        {
            Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

            ShouldPassRule(_ =>
            {
                _.Query = @"query { author(input: { name: ""Quinn"" }) }";
                _.Schema = TypedSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                    {
                        { "Admin", "true" }
                    });
            });
        }

        [Fact]
        public void fails_on_missing_claim_on_input_type()
        {
            Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { author(input: { name: ""Quinn"" }) }";
                _.Schema = TypedSchema();
            });
        }

        [Fact]
        public void passes_with_multiple_policies_on_field_and_single_on_input_type()
        {
            Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));
            Settings.AddPolicy("AdminPolicy", builder => builder.RequireClaim("admin"));
            Settings.AddPolicy("ConfidentialPolicy", builder => builder.RequireClaim("admin"));

            ShouldPassRule(_ =>
            {
                _.Query = @"query { author(input: { name: ""Quinn"" }) project(input: { name: ""TEST"" }) }";
                _.Schema = TypedSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                {
                    { "Admin", "true" }
                });
            });
        }

        [Fact]
        public void Issue61()
        {
            ShouldPassRule(_ =>
            {
                _.Query = @"query { unknown(obj: {id: 7}) }";
                _.Schema = TypedSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                {
                    { "Admin", "true" }
                });
            });
        }

        [Fact]
        public void passes_with_policy_on_connection_type()
        {
            Settings.AddPolicy("ConnectionPolicy", _ => _.RequireClaim("admin"));

            ShouldPassRule(_ =>
            {
                _.Query = @"query { posts { items { id } } }";
                _.Schema = TypedSchema();
                _.User = CreatePrincipal(claims: new Dictionary<string, string>
                {
                    { "Admin", "true" }
                });
            });
        }

        [Fact]
        public void fails_on_missing_claim_on_connection_type()
        {
            Settings.AddPolicy("ConnectionPolicy", _ => _.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { posts { items { id } } }";
                _.Schema = TypedSchema();
                _.User = CreatePrincipal();
            });
        }

        [Fact]
        public void passes_when_field_is_not_included()
        {
            Settings.AddPolicy("FieldPolicy", _ => _.RequireClaim("admin"));

            ShouldPassRule(_ =>
            {
                _.Query = @"query { post @include(if: false) }";
                _.Schema = BasicSchema();
            });
        }

        [Fact]
        public void fails_when_field_is_included()
        {
            Settings.AddPolicy("FieldPolicy", _ => _.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { post @include(if: true) }";
                _.Schema = BasicSchema();
            });
        }

        [Fact]
        public void passes_when_field_is_skipped()
        {
            Settings.AddPolicy("FieldPolicy", _ => _.RequireClaim("admin"));

            ShouldPassRule(_ =>
            {
                _.Query = @"query { post @skip(if: true) }";
                _.Schema = BasicSchema();
            });
        }

        [Fact]
        public void fails_when_field_is_not_skipped()
        {
            Settings.AddPolicy("FieldPolicy", _ => _.RequireClaim("admin"));

            ShouldFailRule(_ =>
            {
                _.Query = @"query { post @skip(if: false) }";
                _.Schema = BasicSchema();
            });
        }

        private static ISchema BasicSchema()
        {
            string defs = @"
                type Query {
                    post(id: ID!): String
                }
            ";

            return Schema.For(defs, builder => builder.Types.Include<BasicQueryWithAttributes>());
        }

        [GraphQLMetadata("Query")]
        [GraphQLAuthorize("ClassPolicy")]
        public class BasicQueryWithAttributes
        {
            [GraphQLAuthorize("FieldPolicy")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "test")]
            public string Post(string id) => "";
        }

        private static ISchema NestedSchema()
        {
            string defs = @"
                type Query {
                    post(id: ID!): Post
                    posts: [Post]
                    postsNonNull: [Post!]!
                }

                type Post {
                    id: ID!
                }
            ";

            return Schema.For(defs, builder =>
            {
                builder.Types.Include<NestedQueryWithAttributes>();
                builder.Types.Include<Post>();
            });
        }

        [GraphQLMetadata("Query")]
        public class NestedQueryWithAttributes
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "test")]
            public Post Post(string id) => null;

            public IEnumerable<Post> Posts() => null;

            public IEnumerable<Post> PostsNonNull() => null;
        }

        [GraphQLAuthorize("PostPolicy")]
        public class Post
        {
            public string Id { get; set; }
        }

        public class PostGraphType : ObjectGraphType<Post>
        {
            public PostGraphType()
            {
                Field(p => p.Id);
            }
        }

        public class Author
        {
            public string Name { get; set; }
        }

        private static ISchema TypedSchema()
        {
            var query = new ObjectGraphType();
            query.Field<StringGraphType>(
                "author",
                arguments: new QueryArguments(new QueryArgument<AuthorInputType> { Name = "input" }),
                resolve: context => "testing"
            );

            query.Connection<PostGraphType>()
                .Name("posts")
                .AuthorizeWith("ConnectionPolicy")
                .Resolve(ctx => new Connection<Post>());

            query.Field<StringGraphType>(
                "project",
                arguments: new QueryArguments(new QueryArgument<AuthorInputType> { Name = "input" }),
                resolve: context => "testing"
            ).AuthorizeWith("AdminPolicy").AuthorizeWith("ConfidentialPolicy");

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
