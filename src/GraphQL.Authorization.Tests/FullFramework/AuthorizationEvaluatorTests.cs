#if (NET46)
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace GraphQL.Authorization.Tests
{
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
            _settings.AddPolicy("MyPolicy", _ =>
            {
                _.RequireClaim("Admin");
            });

            var result = await _evaluator.Evaluate(
                null,
                null,
                null,
                new[] {"MyPolicy"}
            );

            result.Succeeded.ShouldBeFalse();
        }

        [Fact]
        public async Task fails_when_missing_claim()
        {
            _settings.AddPolicy("MyPolicy", _ =>
            {
                _.RequireClaim("Admin");
            });

            var result = await _evaluator.Evaluate(
                CreatePrincipal(),
                null,
                null,
                new[] {"MyPolicy"}
            );

            result.Succeeded.ShouldBeFalse();
        }

        [Fact]
        public async Task succeeds_when_policy_applied()
        {
            _settings.AddPolicy("MyPolicy", _ =>
            {
                _.RequireClaim("Admin");
            });

            var result = await _evaluator.Evaluate(
                CreatePrincipal(claims: new Dictionary<string, string>
                {
                    {"Admin", "true"}
                }),
                null,
                null,
                new[] {"MyPolicy"}
            );

            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public async Task succeeds_with_claim_value()
        {
            _settings.AddPolicy("MyPolicy", _ =>
            {
                _.RequireClaim("Admin", "true");
            });

            var result = await _evaluator.Evaluate(
                CreatePrincipal(claims: new Dictionary<string, string>
                {
                    {"Admin", "true"}
                }),
                null,
                null,
                new[] {"MyPolicy"}
            );

            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public async Task succeeds_when_null_policies()
        {
            _settings.AddPolicy("MyPolicy", _ =>
            {
                _.RequireClaim("Admin");
            });

            var result = await _evaluator.Evaluate(
                CreatePrincipal(claims: new Dictionary<string, string>
                {
                    {"Admin", "true"}
                }),
                null,
                null,
                null
            );

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
            );

            result.Succeeded.ShouldBeTrue();
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
