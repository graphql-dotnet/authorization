using System;
using System.Linq;
using System.Text;
using GraphQL.Language.AST;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    /// <summary>
    /// An error that represents an authorization failure while parsing the document.
    /// </summary>
    public class AuthorizationError : ValidationError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationError"/> class for a specified authorization result.
        /// </summary>
        public AuthorizationError(INode node, ValidationContext context, OperationType? operationType, AuthorizationResult result)
            : this(node, context, GenerateMessage(operationType, result), result)
        {
            OperationType = operationType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationError"/> class for a specified authorization result with a specific error message.
        /// </summary>
        public AuthorizationError(INode node, ValidationContext context, string message, AuthorizationResult result)
            : base(context.Document.OriginalQuery, "6.1.1", message, node == null ? Array.Empty<INode>() : new INode[] { node })
        {
            Code = "authorization";
            AuthorizationResult = result;
        }

        /// <summary>
        /// Returns the result of authorization request.
        /// </summary>
        public virtual AuthorizationResult AuthorizationResult { get; }

        /// <summary>
        /// The GraphQL operation type.
        /// </summary>
        public OperationType? OperationType { get; }

        private static string GenerateMessage(OperationType? operationType, AuthorizationResult result)
        {
            var error = new StringBuilder();
            AppendFailureHeader(error, operationType);

            foreach (var failure in result.Failure.FailedRequirements)
            {
                AppendFailureLine(error, failure);
            }

            return error.ToString();
        }

        private static string GetOperationType(OperationType? operationType)
        {
            return operationType switch
            {
                Language.AST.OperationType.Query => "query",
                Language.AST.OperationType.Mutation => "mutation",
                Language.AST.OperationType.Subscription => "subscription",
                _ => "operation",
            };
        }

        /// <summary>
        /// Appends the error message header for this <see cref="AuthorizationError"/> to the provided <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="error">The error message <see cref="StringBuilder"/>.</param>
        /// <param name="operationType">The GraphQL operation type.</param>
        public static void AppendFailureHeader(StringBuilder error, OperationType? operationType)
        {
            error.Append("You are not authorized to run this ")
                .Append(GetOperationType(operationType))
                .Append('.');
        }

        /// <summary>
        /// Appends a description of the failed <paramref name="authorizationRequirement"/> to the supplied <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="error">The <see cref="StringBuilder"/> which is used to compose the error message.</param>
        /// <param name="authorizationRequirement">The failed <see cref="IAuthorizationRequirement"/>.</param>
        public static void AppendFailureLine(StringBuilder error, IAuthorizationRequirement authorizationRequirement)
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
