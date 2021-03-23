using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class ClaimAuthorizationRequirementTests
    {
        [Fact]
        public async Task produces_error_when_missing_claim_ignoring_value()
        {
            var req = new ClaimsAuthorizationRequirement("Admin");
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, ValidationTestBase.CreatePrincipal());

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeFalse();
            context.HasFailed.ShouldBeFalse();
            context.PendingRequirements.Single().ShouldBe(req);
            //context.Errors.Single().ShouldBe("Required claim 'Admin' is not present.");
        }

        [Fact]
        public async Task produces_error_when_missing_claim_with_single_value()
        {
            var req = new ClaimsAuthorizationRequirement("Admin", "true");
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, ValidationTestBase.CreatePrincipal());

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeFalse();
            context.HasFailed.ShouldBeFalse();
            context.PendingRequirements.Single().ShouldBe(req);
            //context.Errors.Single().ShouldBe("Required claim 'Admin' with any value of 'true' is not present.");
        }

        [Fact]
        public async Task produces_error_when_missing_claim_with_multiple_values()
        {
            var req = new ClaimsAuthorizationRequirement("Admin", "true", "maybe");
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, ValidationTestBase.CreatePrincipal());

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeFalse();
            context.HasFailed.ShouldBeFalse();
            context.PendingRequirements.Single().ShouldBe(req);
            //context.Errors.Single().ShouldBe("Required claim 'Admin' with any value of 'true, maybe' is not present.");
        }

        [Fact]
        public async Task succeeds_when_claim_with_ignoring_value()
        {
            var req = new ClaimsAuthorizationRequirement("Admin");
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "true" } }));

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeTrue();
            context.HasFailed.ShouldBeFalse();
            context.PendingRequirements.Count().ShouldBe(0);
        }

        [Fact]
        public async Task succeeds_when_claim_with_single_value()
        {
            var req = new ClaimsAuthorizationRequirement("Admin", "true");
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "true" } }));

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeTrue();
            context.HasFailed.ShouldBeFalse();
            context.PendingRequirements.Count().ShouldBe(0);
        }

        [Fact]
        public async Task succeeds_when_claim_with_multiple_values()
        {
            var req = new ClaimsAuthorizationRequirement("Admin", "true", "maybe");
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "maybe" } }));

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeTrue();
            context.HasFailed.ShouldBeFalse();
            context.PendingRequirements.Count().ShouldBe(0);
        }
    }
}
