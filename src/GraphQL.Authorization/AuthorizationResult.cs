using System.Collections.Generic;

namespace GraphQL.Authorization
{
    public class AuthorizationResult
    {
        // allocation optimization for green path
        private static readonly AuthorizationResult _success = new AuthorizationResult { Succeeded = true };

        public bool Succeeded { get; private set; }

        public IEnumerable<string> Errors { get; private set; }

        public static AuthorizationResult Success() => _success;

        public static AuthorizationResult Fail(IEnumerable<string> errors) => new AuthorizationResult { Errors = errors };
    }
}
