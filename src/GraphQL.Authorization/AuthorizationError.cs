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
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationError"/> class with the specified parameters.
        /// </summary>
        public AuthorizationError(INode? node, ValidationContext context, OperationType? operationType, string message, AuthorizationResult result)
            : base(context.Document.OriginalQuery, "6.1.1", message, node == null ? Array.Empty<INode>() : new INode[] { node })
        {
            Code = "authorization";
            OperationType = operationType;
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
