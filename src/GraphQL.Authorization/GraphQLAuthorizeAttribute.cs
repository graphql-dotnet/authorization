using System;

namespace GraphQL.Authorization
{
    public class GraphQLAuthorizeAttribute : Attribute
    {
        public string Policy { get; set; }
    }
}