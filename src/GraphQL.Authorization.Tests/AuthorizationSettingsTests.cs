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
                .AddRequirement(new ClaimsAuthorizationRequirement("SuperPlus", Enumerable.Empty<string>()))
            );

            _settings.Policies.Count().ShouldBe(1);

            var policy = _settings.Policies.Single();
            policy.Requirements.Count().ShouldBe(4);
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
            policy.Requirements.Single().ShouldBeOfType<DelegatedRequirement>();
        }

        [Fact]
        public void get_policies()
        {
            _settings.AddPolicy("MyPolicy1", new AuthorizationPolicy(new DelegatedRequirement(c => Task.CompletedTask)));
            _settings.AddPolicy("MyPolicy2", b => b.Require(c => Task.CompletedTask));

            _settings.GetPolicies("a").ShouldBeEmpty();
            _settings.GetPolicies("a", "b").ShouldBeEmpty();
            _settings.GetPolicies(Enumerable.Empty<string>()).ShouldBeEmpty();

            _settings.GetPolicies("MyPolicy1").Count().ShouldBe(1);
            _settings.GetPolicies("a", "MyPolicy1", "b").Count().ShouldBe(1);
            _settings.GetPolicies("a", "MyPolicy2", "MyPolicy1", "b").Count().ShouldBe(2);

            _settings.Policies.Count().ShouldBe(2);
        }

        [Fact]
        public void replace_policy()
        {
            _settings.AddPolicy("MyPolicy1", b => b.RequireAuthenticatedUser());
            _settings.AddPolicy("MyPolicy1", b => b.RequireClaim("claim_777"));

            _settings.Policies.Count().ShouldBe(1);
            var req = _settings.Policies.Single().Requirements.Single().ShouldBeOfType<ClaimsAuthorizationRequirement>();
            req.ClaimType.ShouldBe("claim_777");
            req.DisplayValues.ShouldBeNull();
        }
    }
}
