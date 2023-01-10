using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.Authorization.Tests;

public class AuthorizationValidationRuleTests : ValidationTestBase
{
    [Fact]
    public void class_policy_success()
    {
        Settings.AddPolicy("ClassPolicy", builder => builder.RequireClaim("admin"));
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.Query = "query { post }";
            config.Schema = BasicSchema();
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            });
        });
    }

    [Fact]
    public void class_policy_fail()
    {
        Settings.AddPolicy("ClassPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query { post }";
            config.Schema = BasicSchema();
        });
    }

    [Fact]
    public void field_policy_success()
    {
        Settings.AddPolicy("ClassPolicy", builder => builder.RequireClaim("admin"));
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.Query = "query { post }";
            config.Schema = BasicSchema();
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
                {
                    { "Admin", "true" }
                });
        });
    }

    [Fact]
    public void field_policy_fail()
    {
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query { post }";
            config.Schema = BasicSchema();
        });
    }

    [Fact]
    public void nested_type_policy_success()
    {
        Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.Query = "query { post }";
            config.Schema = NestedSchema();
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            });
        });
    }

    [Fact]
    public void nested_type_policy_fail()
    {
        Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query { post }";
            config.Schema = NestedSchema();
        });
    }

    [Fact]
    public void nested_type_list_policy_fail()
    {
        Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query { posts }";
            config.Schema = NestedSchema();
        });
    }

    // https://github.com/graphql-dotnet/authorization/issues/5
    [Theory]
    [InlineData("c", "query p { posts } query c { comment }")]
    [InlineData(null, "query c { comment } query p { posts }")]
    public void issue5_should_pass(string operationName, string query)
    {
        Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.OperationName = operationName;
            config.Query = query;
            config.Schema = NestedSchema();
        });
    }

    // https://github.com/graphql-dotnet/authorization/issues/5
    [Theory]
    [InlineData("query a { article { id } } query b { article { ...frag } } fragment frag on Article { content }")]
    [InlineData("query a { article { ...frag1 author } } query b { article { ...frag2 } } fragment frag1 on Article { id } fragment frag2 on Article { content }")]
    public void issue5_with_fragment_should_pass(string query)
    {
        Settings.AddPolicy("AdminPolicy", builder => builder.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.Query = query;
            config.Schema = TypedSchema();
        });
    }

    // https://github.com/graphql-dotnet/authorization/issues/5
    [Fact]
    public void issue5_with_fragment_should_fail()
    {
        Settings.AddPolicy("AdminPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query a { article { ...frag } } query b { article { ...frag } } fragment frag on Article { content }";
            config.Schema = TypedSchema();
            config.ValidateResult = result => _ = result.Errors.Single(x => x.Message == $"You are not authorized to run this query.\nRequired claim 'admin' is not present.");
        });
    }

    [Fact]
    public void nested_fragment_should_fail()
    {
        Settings.AddPolicy("AdminPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query a { article { ...frag } } query b { article { ...frag } } fragment frag on Article { ...frag2 } fragment frag2 on Article { content }";
            config.Schema = TypedSchema();
            config.ValidateResult = result => _ = result.Errors.Single(x => x.Message == $"You are not authorized to run this query.\nRequired claim 'admin' is not present.");
        });
    }

    [Fact]
    public void nested_type_list_non_null_policy_fail()
    {
        Settings.AddPolicy("PostPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query { postsNonNull }";
            config.Schema = NestedSchema();
        });
    }

    [Fact]
    public void passes_with_claim_on_input_type()
    {
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.Query = """query { author(input: { name: "Quinn" }) }""";
            config.Schema = TypedSchema();
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            });
        });
    }

    [Fact]
    public void fails_on_missing_claim_on_input_type()
    {
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = """query { author(input: { name: "Quinn" }) }""";
            config.Schema = TypedSchema();
        });
    }

    [Fact]
    public void passes_with_multiple_policies_on_field_and_single_on_input_type()
    {
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));
        Settings.AddPolicy("AdminPolicy", builder => builder.RequireClaim("admin"));
        Settings.AddPolicy("ConfidentialPolicy", builder => builder.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.Query = """query { author(input: { name: "Quinn" }) project(input: { name: "TEST" }) }""";
            config.Schema = TypedSchema();
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            });
        });
    }

    [Fact]
    public void Issue61()
    {
        ShouldPassRule(config =>
        {
            config.Query = "query { unknown(obj: {id: 7}) }";
            config.Schema = TypedSchema();
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            });
        });
    }

    [Fact]
    public void passes_with_claim_on_variable_type()
    {
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.Query = "query Author($input: AuthorInputType!) { author(input: $input) }";
            config.Schema = TypedSchema();
            config.Variables = new Inputs(new Dictionary<string, object?>()
            {
                {
                    "input",
                    new Dictionary<string,object>{ { "name","Quinn" } }
                }
            });
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            });
        });
    }

    [Fact]
    public void passes_with_claim_on_variable_type_without_inputs_but_fails_later_either()
    {
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.ValidateResult = result =>
            {
                // no auth error on null inputs, only INVALID_VALUE
                result.Errors.Count.ShouldBe(1);
                result.Errors[0].Code.ShouldBe("INVALID_VALUE");
                result.Errors[0].Message.ShouldBe("Variable '$input' is invalid. No value provided for a non-null variable.");
            };
            config.Query = "query Author($input: AuthorInputType!) { author(input: $input) }";
            config.Schema = TypedSchema();
            config.Variables = null;
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            });
        });
    }

    [Fact]
    public void fails_on_missing_claim_on_variable_type()
    {
        Settings.AddPolicy("FieldPolicy", builder => builder.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query Author($input: AuthorInputType!) { author(input: $input) }";
            config.Schema = TypedSchema();
            config.Variables = new Inputs(new Dictionary<string, object?>()
            {
                {
                    "input",
                    new Dictionary<string,object>{ { "name","Quinn" } }
                }
            });
        });
    }

    [Fact]
    public void passes_with_policy_on_connection_type()
    {
        Settings.AddPolicy("ConnectionPolicy", _ => _.RequireClaim("admin"));

        ShouldPassRule(config =>
        {
            config.Query = "query { posts { items { id } } }";
            config.Schema = TypedSchema();
            config.User = CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            });
        });
    }

    [Fact]
    public void fails_on_missing_claim_on_connection_type()
    {
        Settings.AddPolicy("ConnectionPolicy", _ => _.RequireClaim("admin"));

        ShouldFailRule(config =>
        {
            config.Query = "query { posts { items { id } } }";
            config.Schema = TypedSchema();
            config.User = CreatePrincipal();
        });
    }

    private static ISchema BasicSchema()
    {
        string defs = """
            type Query {
              post(id: ID!): String
            }
            """;

        return Schema.For(defs, builder => builder.Types.Include<BasicQueryWithAttributes>());
    }

    [GraphQLMetadata("Query")]
    [Authorize("ClassPolicy")]
    public class BasicQueryWithAttributes
    {
        [Authorize("FieldPolicy")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "test")]
        public string Post(string id) => "";
    }

    private static ISchema NestedSchema()
    {
        string defs = """
            type Query {
              post(id: ID!): Post
              posts: [Post]
              postsNonNull: [Post!]!
              comment: String
            }

            type Post {
              id: ID!
            }
            """;

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
        public Post? Post(string id) => null;

        public IEnumerable<Post>? Posts() => null;

        public IEnumerable<Post>? PostsNonNull() => null;

        public string? Comment() => null;
    }

    [Authorize("PostPolicy")]
    public class Post
    {
        public string? Id { get; set; }
    }

    public class PostGraphType : ObjectGraphType<Post>
    {
        public PostGraphType()
        {
            Field(p => p.Id);
        }
    }

    public class Article
    {
        public string? Id { get; set; }

        public string? Author { get; set; }

        public string? Content { get; set; }
    }

    public class ArticleGraphType : ObjectGraphType<Article>
    {
        public ArticleGraphType()
        {
            Field(p => p.Id);
            Field(p => p.Author);
            Field(p => p.Content).AuthorizeWithPolicy("AdminPolicy");
        }
    }

    public class Author
    {
        public string? Name { get; set; }
    }

    private static ISchema TypedSchema()
    {
        var query = new ObjectGraphType();
        query.Field<StringGraphType>(
            "author",
            arguments: new QueryArguments(new QueryArgument<AuthorInputType> { Name = "input" }),
            resolve: _ => "testing"
        );

        query.Connection<PostGraphType>()
            .Name("posts")
            .AuthorizeWithPolicy("ConnectionPolicy")
            .Resolve(_ => new Connection<Post>());

        query.Field<StringGraphType>(
            "project",
            arguments: new QueryArguments(new QueryArgument<AuthorInputType> { Name = "input" }),
            resolve: _ => "testing"
        ).AuthorizeWithPolicy("AdminPolicy").AuthorizeWithPolicy("ConfidentialPolicy");

        query.Field<ArticleGraphType>(
            "article",
            resolve: _ => null
        );

        return new Schema { Query = query };
    }

    public class AuthorInputType : InputObjectGraphType<Author>
    {
        public AuthorInputType()
        {
            Field(x => x.Name).AuthorizeWithPolicy("FieldPolicy");
        }
    }
}
