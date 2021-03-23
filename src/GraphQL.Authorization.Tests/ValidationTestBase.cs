using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GraphQL.Execution;
using GraphQL.Validation;
using Shouldly;

namespace GraphQL.Authorization.Tests
{
    public class ValidationTestBase
    {
        public ValidationTestBase()
        {
            Settings = new AuthorizationSettings();
            Rule = new AuthorizationValidationRule(new DefaultAuthorizationService(), new DefaultClaimsPrincipalAccessor(), new DefaultAuthorizationPolicyProvider(Settings));
        }

        protected AuthorizationSettings Settings { get; }

        protected AuthorizationValidationRule Rule { get; }

        protected void ShouldPassRule(Action<ValidationTestConfig> configure)
        {
            var config = new ValidationTestConfig();
            config.Rules.Add(Rule);
            configure(config);

            config.Rules.Any().ShouldBeTrue("Must provide at least one rule to validate against.");

            config.Schema.Initialize();

            var result = Validate(config);

            string message = "";
            if (result.Errors?.Any() == true)
            {
                message = string.Join(", ", result.Errors.Select(x => x.Message));
            }
            result.IsValid.ShouldBeTrue(message);
            config.ValidateResult(result);
        }

        protected void ShouldFailRule(Action<ValidationTestConfig> configure)
        {
            var config = new ValidationTestConfig();
            config.Rules.Add(Rule);
            configure(config);

            config.Rules.Any().ShouldBeTrue("Must provide at least one rule to validate against.");

            config.Schema.Initialize();

            var result = Validate(config);

            result.IsValid.ShouldBeFalse("Expected validation errors though there were none.");
            config.ValidateResult(result);
        }

        private static IValidationResult Validate(ValidationTestConfig config)
        {
            var userContext = new GraphQLUserContext { User = config.User };
            var documentBuilder = new GraphQLDocumentBuilder();
            var document = documentBuilder.Build(config.Query);
            var validator = new DocumentValidator();
            return validator.ValidateAsync(config.Schema, document, document.Operations.First().Variables, config.Rules, userContext, config.Inputs).GetAwaiter().GetResult().validationResult;
        }

        internal static ClaimsPrincipal CreatePrincipal(string? authenticationType = null, IDictionary<string, string>? claims = null)
        {
            var claimsList = new List<Claim>();

            if (claims != null)
            {
                foreach (var c in claims)
                    claimsList.Add(new Claim(c.Key, c.Value));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claimsList, authenticationType));
        }
    }
}
