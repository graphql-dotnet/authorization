namespace GraphQL.Authorization;

/// <summary>
/// Implements an <see cref="IAuthorizationRequirement"/> which requires an instance of the specified
/// claim type, and, if allowed values are specified, the claim value must be any of the allowed values.
/// </summary>
public class ClaimAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Creates a new instance of <see cref="ClaimAuthorizationRequirement"/> with
    /// the specified claim type.
    /// </summary>
    public ClaimAuthorizationRequirement(string claimType)
        : this(claimType, (IEnumerable<string>?)null, null)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ClaimAuthorizationRequirement"/> with
    /// the specified claim type and optional list of claim values, which, if present,
    /// the claim must match.
    /// </summary>
    public ClaimAuthorizationRequirement(string claimType, IEnumerable<string> allowedValues)
        : this(claimType, allowedValues, null)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ClaimAuthorizationRequirement"/> with
    /// the specified claim type and optional list of claim values, which, if present,
    /// the claim must match.
    /// </summary>
    public ClaimAuthorizationRequirement(string claimType, params string[] allowedValues)
        : this(claimType, allowedValues, null)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ClaimAuthorizationRequirement"/> with
    /// the specified claim type and optional list of claim values, which, if present,
    /// the claim must match. Additional <paramref name="displayValues"/> argument
    /// specifies the set of displayed claim values that will be used to generate an
    /// error message if the requirement is not met.
    /// </summary>
    public ClaimAuthorizationRequirement(string claimType, IEnumerable<string>? allowedValues, IEnumerable<string>? displayValues)
    {
        ClaimType = claimType ?? throw new ArgumentNullException(nameof(claimType));
        AllowedValues = allowedValues ?? Enumerable.Empty<string>();
        DisplayValues = displayValues;
    }

    /// <summary>
    /// Claim type that claims principal from <see cref="AuthorizationContext"/> should have.
    /// </summary>
    public string ClaimType { get; }

    /// <summary>
    /// List of claim values, which, if present, the claim must match.
    /// </summary>
    public IEnumerable<string> AllowedValues { get; }

    /// <summary>
    /// Specifies the set of displayed claim values that will be used
    /// to generate an error message if the requirement is not met.
    /// If null then values from <see cref="AllowedValues"/> are used.
    /// </summary>
    public IEnumerable<string>? DisplayValues { get; }

    /// <inheritdoc />
    public Task Authorize(AuthorizationContext context)
    {
        bool found = false;

        if (context.User != null)
        {
            if (AllowedValues == null || !AllowedValues.Any())
            {
                found = context.User.Claims.Any(
                    claim => string.Equals(claim.Type, ClaimType, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                found = context.User.Claims.Any(
                    claim => string.Equals(claim.Type, ClaimType, StringComparison.OrdinalIgnoreCase)
                         && AllowedValues.Contains(claim.Value, StringComparer.Ordinal));
            }
        }

        if (!found)
        {
            if (AllowedValues != null && AllowedValues.Any())
            {
                string values = string.Join(", ", DisplayValues ?? AllowedValues);
                context.ReportError($"Required claim '{ClaimType}' with any value of '{values}' is not present.");
            }
            else
            {
                context.ReportError($"Required claim '{ClaimType}' is not present.");
            }
        }

        return Task.CompletedTask;
    }
}
