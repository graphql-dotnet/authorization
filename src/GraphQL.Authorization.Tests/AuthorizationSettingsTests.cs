using System.Linq;
using Shouldly;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class AuthorizationSettingsTests
    {
        private readonly AuthorizationSettings _settings;

        public AuthorizationSettingsTests()
        {
            _settings = new AuthorizationSettings();
        }

        [Fact]
        public void can_add_a_claim_policy()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

            _settings.Policies.Count().ShouldBe(1);

            var policy = _settings.Policies.Single();
            policy.Requirements.Single().ShouldBeOfType<ClaimsAuthorizationRequirement>();
        }
    }
}
