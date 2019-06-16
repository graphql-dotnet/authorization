using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.Authorization
{
    public static class AuthorizationMetadataExtensions
    {
        public static readonly string PolicyKey = "Authorization__Policies";

        public static bool RequiresAuthorization(this IProvideMetadata type)
        {
            return GetPolicies(type).Any();
        }

        public static Task<AuthorizationResult> Authorize(
            this IProvideMetadata type,
            ClaimsPrincipal principal,
            object userContext,
            Dictionary<string, object> inputVariables,
            IAuthorizationEvaluator evaluator)
        {
            var list = GetPolicies(type);
            return evaluator.Evaluate(principal, userContext, inputVariables, list);
        }

        public static void AuthorizeWith(this IProvideMetadata type, params string[] policies)
        {
            var list = GetPolicies(type);
            foreach (string policy in policies)
            {
                if (!list.Contains(policy))
                {
                    list.Add(policy);
                }
            }
            type.Metadata[PolicyKey] = list;
        }

        public static FieldBuilder<TSourceType, TReturnType> AuthorizeWith<TSourceType, TReturnType>(
            this FieldBuilder<TSourceType, TReturnType> builder, string policy)
        {
            builder.FieldType.AuthorizeWith(policy);
            return builder;
        }

        public static List<string> GetPolicies(this IProvideMetadata type)
        {
            return type.GetMetadata(PolicyKey, new List<string>());
        }
    }
}
