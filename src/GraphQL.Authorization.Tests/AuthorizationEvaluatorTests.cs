namespace GraphQL.Authorization.Tests;

public class AuthorizationEvaluatorTests
{
    private readonly AuthorizationEvaluator _evaluator;
    private readonly AuthorizationSettings _settings;

    public AuthorizationEvaluatorTests()
    {
        _settings = new AuthorizationSettings();
        _evaluator = new AuthorizationEvaluator(_settings);
    }

    [Fact]
    public async Task fails_with_null_principal()
    {
        _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

        var result = await _evaluator.Evaluate(
            null,
            null,
            null,
            new[] { "MyPolicy" }
        ).ConfigureAwait(false);

        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task fails_when_missing_claim()
    {
        _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

        var result = await _evaluator.Evaluate(
            ValidationTestBase.CreatePrincipal(),
            null,
            null,
            new[] { "MyPolicy" }
        ).ConfigureAwait(false);

        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task fails_when_missing_policy()
    {
        _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

        var result = await _evaluator.Evaluate(
            ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            }),
            null,
            null,
            new[] { "PolicyDoesNotExist" }
        ).ConfigureAwait(false);

        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task succeeds_when_policy_applied()
    {
        _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

        var result = await _evaluator.Evaluate(
            ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            }),
            null,
            null,
            new[] { "MyPolicy" }
        ).ConfigureAwait(false);

        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task succeeds_with_claim_value()
    {
        _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin", "true"));

        var result = await _evaluator.Evaluate(
            ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            }),
            null,
            null,
            new[] { "MyPolicy" }
        ).ConfigureAwait(false);

        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task succeeds_when_null_policies()
    {
        _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

        var result = await _evaluator.Evaluate(
            ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            }),
            null,
            null,
            null
        ).ConfigureAwait(false);

        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task succeeds_when_empty_policies()
    {
        _settings.AddPolicy("MyPolicy", _ => { });

        var result = await _evaluator.Evaluate(
            ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string>
            {
                { "Admin", "true" }
            }),
            null,
            null,
            Array.Empty<string>()
        ).ConfigureAwait(false);

        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task succeeds_when_null_principal()
    {
        var result = await _evaluator.Evaluate(
            null,
            null,
            null,
            null
        ).ConfigureAwait(false);

        result.Succeeded.ShouldBeTrue();
    }
}
