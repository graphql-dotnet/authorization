using System;
using System.Linq;
using System.Threading.Tasks;
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
        public void throw_if_add_null_delegate()
        {
            Should.Throw<ArgumentNullException>(() => _settings.AddPolicy("MyPolicy", (Action<AuthorizationPolicyBuilder>)null!));
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

        [Fact]
        public void can_add_policy_instance()
        {
            _settings.AddPolicy("MyPolicy", new AuthorizationPolicy(new DelegatedRequirement(c => Task.CompletedTask)));

            _settings.Policies.Count().ShouldBe(1);

            var policy = _settings.Policies.Single();
            policy.Requirements.Count().ShouldBe(1);
            policy.Requirements.Single().ShouldBeOfType<AuthenticatedUserRequirement>();
        }

        [Fact]
        public void get_policies()
        {
            _settings.AddPolicy("MyPolicy", new AuthorizationPolicy(new DelegatedRequirement(c => Task.CompletedTask)));

            _settings.GetPolicies("a").ShouldBeEmpty();
            _settings.GetPolicies("a", "b").ShouldBeEmpty();
            _settings.GetPolicies(Enumerable.Empty<string>()).ShouldBeEmpty();

            _settings.GetPolicies("MyPolicy").Count().ShouldBe(1);
            _settings.GetPolicies("a", "MyPolicy", "b").Count().ShouldBe(1);

            _settings.Policies.Count().ShouldBe(1);
        }
    }
}
