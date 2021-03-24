using System;
using GraphQL.Language.AST;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    /// <summary>
    /// An error that represents an authorization failure while parsing the document.
    /// </summary>
    public class AuthorizationError : ValidationError
    {
        private static readonly AuthorizationErrorMessageBuilder _builder = new AuthorizationErrorMessageBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationError"/> class for a specified authorization result.
        /// </summary>
        public AuthorizationError(INode? node, ValidationContext context, OperationType? operationType, AuthorizationResult result)
            : this(node, context, _builder.Build(operationType, result), result)
        {
            OperationType = operationType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationError"/> class for a specified authorization result with a specific error message.
        /// </summary>
        public AuthorizationError(INode? node, ValidationContext context, string message, AuthorizationResult result)
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
    }
}
