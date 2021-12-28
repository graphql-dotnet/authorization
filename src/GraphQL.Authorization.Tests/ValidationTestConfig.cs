using System;
using System.Collections.Generic;
using System.Security.Claims;
using GraphQL.Types;
using GraphQL.Validation;

namespace GraphQL.Authorization.Tests
{
    public class ValidationTestConfig
    {
        public string OperationName { get; set; } = null!;

        public string Query { get; set; } = null!;

        public ISchema Schema { get; set; } = null!;

        public List<IValidationRule> Rules { get; set; } = new List<IValidationRule>();

        public ClaimsPrincipal? User { get; set; }

        public Inputs? Variables { get; set; }

        public Action<IValidationResult> ValidateResult = _ => { };
    }
}
