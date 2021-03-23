using System;
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
        public void throw_if_add_null_requirement()
        {
            Should.Throw<ArgumentNullException>(() => _settings.AddPolicy("MyPolicy", builder => builder.AddRequirement(null!)));
        }

        [Fact]
        public void can_add_a_claim_policy()
        {
            _settings.AddPolicy("MyPolicy", builder => builder
                .RequireClaim("Admin")
                .RequireClaim("SuperAdmin", "Super1", "Super2")
                .RequireClaim("SuperDuperAdmin", new[] { "Super1", "Super2" }, new[] { "Display1", "Display2" })
            );

            _settings.Policies.Count().ShouldBe(1);

            var policy = _settings.Policies.Single();
            policy.Requirements.Count().ShouldBe(3);
            policy.Requirements.ToList().ForEach(r => r.ShouldBeOfType<ClaimsAuthorizationRequirement>());
        }

        [Fact]
        public void can_add_authenticated_user_policy()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireAuthenticatedUser());

            _settings.Policies.Count().ShouldBe(1);

            var policy = _settings.Policies.Single();
            policy.Requirements.Count().ShouldBe(1);
            policy.Requirements.Single().ShouldBeOfType<AuthenticatedUserRequirement>();
        }
    }
}
