using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using GraphQL.Validation;

namespace GraphQL.Authorization
{
    public interface IUserContextAccessor
    {
        ClaimsPrincipal Get(ValidationContext context);
    }
}
