using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class AuthenticatedUserRequirementTests
    {
        [Fact]
        public async Task produces_error_when_not_authenticated()
        {
            var req = new AuthenticatedUserRequirement();

            var context = new AuthorizationContext
            {
                User = ValidationTestBase.CreatePrincipal()
            };

            await req.Authorize(context);

            context.HasErrors.ShouldBeTrue();
            context.Errors.Single().ShouldBe("An authenticated user is required.");
        }

        [Fact]
        public async Task no_errors_when_authenticated()
        {
            var req = new AuthenticatedUserRequirement();

            var context = new AuthorizationContext
            {
                User = ValidationTestBase.CreatePrincipal("jwt")
            };

            await req.Authorize(context);

            context.HasErrors.ShouldBeFalse();
        }
    }
}
