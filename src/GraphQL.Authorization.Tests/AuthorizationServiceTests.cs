using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace GraphQL.Authorization.Tests
{
    public class AuthorizationServiceTests
    {
        private readonly DefaultAuthorizationService _authorizationService;
        private readonly AuthorizationSettings _settings;

        public AuthorizationServiceTests()
        {
            _settings = new AuthorizationSettings();
            _authorizationService = new DefaultAuthorizationService();
        }

        private IAuthorizationContext CreateAuthorizationContext(
            ClaimsPrincipal? principal,
            IDictionary<string, object>? userContext,
            IReadOnlyDictionary<string, object>? inputs,
            string? requiredPolicy)
        {
            return new DefaultAuthorizationContext(new DefaultAuthorizationPolicyProvider(_settings).GetPolicy(requiredPolicy!)!, principal)
            {
                UserContext = userContext,
                Inputs = inputs,
            };
        }

        [Fact]
        public async Task fails_with_null_principal()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

            var result = await _authorizationService.AuthorizeAsync(CreateAuthorizationContext(
                null,
                null,
                null,
                "MyPolicy"
            ));

            result.Succeeded.ShouldBeFalse();
        }

        [Fact]
        public async Task fails_when_missing_claim()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

            var result = await _authorizationService.AuthorizeAsync(CreateAuthorizationContext(
                ValidationTestBase.CreatePrincipal(),
                null,
                null,
                "MyPolicy"
            ));

            result.Succeeded.ShouldBeFalse();
        }

        [Fact]
        public void throws_when_missing_policy()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

            Should.Throw<ArgumentNullException>(() => CreateAuthorizationContext(
                ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "true" } }),
                null,
                null,
                "PolicyDoesNotExist"
            )).ParamName.ShouldBe("policy");
        }

        [Fact]
        public void throws_when_null_policy()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

            Should.Throw<ArgumentNullException>(() => CreateAuthorizationContext(
                ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "true" } }),
                null,
                null,
                null
            )).ParamName.ShouldBe("policy");
        }

        [Fact]
        public async Task succeeds_when_policy_applied()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin"));

            var result = await _authorizationService.AuthorizeAsync(CreateAuthorizationContext(
                ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "true" } }),
                null,
                null,
                "MyPolicy"
            ));

            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public async Task succeeds_with_claim_value()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin", "true"));

            var result = await _authorizationService.AuthorizeAsync(CreateAuthorizationContext(
                ValidationTestBase.CreatePrincipal(claims: new Dictionary<string, string> { { "Admin", "true" } }),
                null,
                null,
                "MyPolicy"
            ));

            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public async Task fails_when_null_principal()
        {
            _settings.AddPolicy("MyPolicy", builder => builder.RequireClaim("Admin", "true"));

            var result = await _authorizationService.AuthorizeAsync(CreateAuthorizationContext(
                null,
                null,
                null,
                "MyPolicy"
            ));

            result.Succeeded.ShouldBeFalse();
        }
    }
}
