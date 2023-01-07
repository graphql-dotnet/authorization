using System.Security.Claims;
using GraphQL.Execution;
using GraphQL.Validation;
using GraphQLParser;

namespace GraphQL.Authorization.Tests;

public class ValidationTestBase
{
    public ValidationTestBase()
    {
        Settings = new AuthorizationSettings();
        Rule = new AuthorizationValidationRule(new AuthorizationEvaluator(Settings));
    }

    protected AuthorizationValidationRule Rule { get; }

    protected AuthorizationSettings Settings { get; }

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
        return validator.ValidateAsync(new ValidationOptions
        {
            Schema = config.Schema,
            Document = document,
            Operation = document.OperationWithName(config.OperationName)!,
            Rules = config.Rules,
            Variables = config.Variables ?? Inputs.Empty,
            UserContext = userContext
        }).GetAwaiter().GetResult().validationResult;
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
