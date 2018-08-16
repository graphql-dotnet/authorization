#if (NET46)
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            var req = new ClaimAuthorizationRequirement("Admin");

            var context = new AuthorizationContext();
            context.User = CreatePrincipal();

            await req.Authorize(context);

            context.HasErrors.ShouldBeTrue();
            context.Errors.Single().ShouldBe("Required claim 'Admin' is not present.");
        }

        [Fact]
        public async Task produces_error_when_missing_claim_with_single_value()
        {
            var req = new ClaimAuthorizationRequirement("Admin", new[] {"true"});

            var context = new AuthorizationContext();
            context.User = CreatePrincipal();

            await req.Authorize(context);

            context.HasErrors.ShouldBeTrue();
            context.Errors.Single().ShouldBe("Required claim 'Admin' with any value of 'true' is not present.");
        }

        [Fact]
        public async Task produces_error_when_missing_claim_with_multiple_values()
        {
            var req = new ClaimAuthorizationRequirement("Admin", new[] {"true", "maybe"});

            var context = new AuthorizationContext();
            context.User = CreatePrincipal();

            await req.Authorize(context);

            context.HasErrors.ShouldBeTrue();
            context.Errors.Single().ShouldBe("Required claim 'Admin' with any value of 'true, maybe' is not present.");
        }

        [Fact]
        public async Task succeeds_when_claim_with_ignoring_value()
        {
            var req = new ClaimAuthorizationRequirement("Admin");

            var context = new AuthorizationContext();
            context.User = CreatePrincipal(claims: new Dictionary<string, string> {{"Admin", "true"}});

            await req.Authorize(context);

            context.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public async Task succeeds_when_claim_with_single_value()
        {
            var req = new ClaimAuthorizationRequirement("Admin", new[] {"true"});

            var context = new AuthorizationContext();
            context.User = CreatePrincipal(claims: new Dictionary<string, string> {{"Admin", "true"}});

            await req.Authorize(context);

            context.HasErrors.ShouldBeFalse();
        }

        [Fact]
        public async Task succeeds_when_claim_with_multiple_values()
        {
            var req = new ClaimAuthorizationRequirement("Admin", new[] {"true", "maybe"});

            var context = new AuthorizationContext();
            context.User = CreatePrincipal(claims: new Dictionary<string, string> {{"Admin", "maybe"}});

            await req.Authorize(context);

            context.HasErrors.ShouldBeFalse();
        }

        private ClaimsPrincipal CreatePrincipal(string authenticationType = null, IDictionary<string, string> claims = null)
        {
            var claimsList = new List<Claim>();

            claims?.Apply(c =>
            {
                claimsList.Add(new Claim(c.Key, c.Value));
            });

            return new ClaimsPrincipal(new ClaimsIdentity(claimsList, authenticationType));
        }
    }
}
#endif
