using System.Linq;
using System.Text;
using GraphQL.Language.AST;

namespace GraphQL.Authorization
{
    /// <summary>
    /// Builds error message for <see cref="AuthorizationError"/> and its descendants.
    /// </summary>
    public class AuthorizationErrorMessageBuilder
    {
        /// <summary>
        /// Builds error message for the specified operation type and authorization result.
        /// </summary>
        public virtual string Build(OperationType? operationType, AuthorizationResult result)
        {
            var error = new StringBuilder();
            AppendFailureHeader(error, operationType);

            if (result.Failure != null)
            {
                foreach (var failure in result.Failure.FailedRequirements)
                {
                    AppendFailureLine(error, failure);
                }
            }

            return error.ToString();
        }

        /// <summary>
        /// Appends the error message header for this <see cref="AuthorizationError"/> to the provided <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="error">The error message <see cref="StringBuilder"/>.</param>
        /// <param name="operationType">The GraphQL operation type.</param>
        public virtual void AppendFailureHeader(StringBuilder error, OperationType? operationType)
        {
            error.Append("You are not authorized to run this ")
                .Append(GetOperationType(operationType))
                .Append('.');

            static string GetOperationType(OperationType? operationType)
            {
                return operationType switch
                {
                    OperationType.Query => "query",
                    OperationType.Mutation => "mutation",
                    OperationType.Subscription => "subscription",
                    _ => "operation",
                };
            }
        }

        /// <summary>
        /// Appends a description of the failed <paramref name="authorizationRequirement"/> to the supplied <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="error">The <see cref="StringBuilder"/> which is used to compose the error message.</param>
        /// <param name="authorizationRequirement">The failed <see cref="IAuthorizationRequirement"/>.</param>
        public virtual void AppendFailureLine(StringBuilder error, IAuthorizationRequirement authorizationRequirement)
        {
            error.AppendLine();

            switch (authorizationRequirement)
            {
                case DefinedPolicyRequirement definedPolicyRequirement:
                    error.Append($"Required policy '{definedPolicyRequirement.PolicyName}' is not present.");
                    break;

                case AuthenticatedUserRequirement _:
                    error.Append("An authenticated user is required.");
                    break;

                case ClaimsAuthorizationRequirement claimsAuthorizationRequirement:
                    error.Append("Required claim '");
                    error.Append(claimsAuthorizationRequirement.ClaimType);
                    if (claimsAuthorizationRequirement.AllowedValues == null || !claimsAuthorizationRequirement.AllowedValues.Any())
                    {
                        error.Append("' is not present.");
                    }
                    else
                    {
                        error.Append("' with any value of '");
                        error.Append(string.Join(", ", claimsAuthorizationRequirement.AllowedValues ?? claimsAuthorizationRequirement.DisplayValues));
                        error.Append("' is not present.");
                    }
                    break;
            }
        }
    }
}
