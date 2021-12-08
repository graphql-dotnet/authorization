using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class AuthenticatedUserRequirementTests
    {
        [Fact]
        public async Task produces_error_when_user_is_null()
        {
            var req = new AuthenticatedUserRequirement();
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, null);

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeFalse();
            context.HasFailed.ShouldBeFalse();
            context.PendingRequirements.Single().ShouldBe(req);
            //context.Errors.Single().ShouldBe("An authenticated user is required.");
        }

        [Fact]
        public async Task produces_error_when_not_authenticated()
        {
            var req = new AuthenticatedUserRequirement();
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, ValidationTestBase.CreatePrincipal());

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeFalse();
            context.HasFailed.ShouldBeFalse();
            context.PendingRequirements.Single().ShouldBe(req);
            //context.Errors.Single().ShouldBe("An authenticated user is required.");
        }

        [Fact]
        public async Task no_errors_when_authenticated()
        {
            var req = new AuthenticatedUserRequirement();
            var policy = new AuthorizationPolicy(req);
            var context = new DefaultAuthorizationContext(policy, ValidationTestBase.CreatePrincipal("jwt"));

            await req.Authorize(context);

            context.HasSucceeded.ShouldBeTrue();
            context.HasFailed.ShouldBeFalse();
        }
    }
}
