namespace GraphQL.Authorization.Tests;

public class ClaimAuthorizationRequirementTests
{
    [Fact]
    public async Task produces_error_when_missing_claim_ignoring_value()
    {
        var req = new ClaimAuthorizationRequirement("Admin");

        var context = new AuthorizationContext
        {
            User = ValidationTestBase.CreatePrincipal()
        };

        await req.Authorize(context).ConfigureAwait(false);

        context.HasErrors.ShouldBeTrue();
        context.Errors.Single().ShouldBe("Required claim 'Admin' is not present.");
    }

    [Fact]
    public async Task produces_error_when_missing_claim_with_single_value()
    {
        var req = new ClaimAuthorizationRequirement("Admin", "true");

        var context = new AuthorizationContext
        {
            User = ValidationTestBase.CreatePrincipal()
        };

        await req.Authorize(context).ConfigureAwait(false);

        context.HasErrors.ShouldBeTrue();
        context.Errors.Single().ShouldBe("Required claim 'Admin' with any value of 'true' is not present.");
    }

    [Fact]
    public async Task produces_error_when_missing_claim_with_multiple_values()
    {
        var req = new ClaimAuthorizationRequirement("Admin", "true", "maybe");

        var context = new AuthorizationContext
        {
            User = ValidationTestBase.CreatePrincipal()
        };

        await req.Authorize(context).ConfigureAwait(false);

        context.HasErrors.ShouldBeTrue();
        context.Errors.Single().ShouldBe("Required claim 'Admin' with any value of 'true, maybe' is not present.");
    }

    [Fact]
    public async Task succeeds_when_claim_with_ignoring_value()
    {
        var req = new ClaimAuthorizationRequirement("Admin");

        var context = new AuthorizationContext
        {
            User = ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "true" } })
        };

        await req.Authorize(context).ConfigureAwait(false);

        context.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task succeeds_when_claim_with_single_value()
    {
        var req = new ClaimAuthorizationRequirement("Admin", "true");

        var context = new AuthorizationContext
        {
            User = ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "true" } })
        };

        await req.Authorize(context).ConfigureAwait(false);

        context.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public async Task succeeds_when_claim_with_multiple_values()
    {
        var req = new ClaimAuthorizationRequirement("Admin", "true", "maybe");

        var context = new AuthorizationContext
        {
            User = ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "maybe" } })
        };

        await req.Authorize(context).ConfigureAwait(false);

        context.HasErrors.ShouldBeFalse();
    }
}
