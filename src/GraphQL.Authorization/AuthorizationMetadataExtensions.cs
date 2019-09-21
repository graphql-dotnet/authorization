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

        public static IEnumerable<string> GetPolicies(this IProvideMetadata type)
        {
            return type.GetMetadata(PolicyKey, Enumerable.Empty<string>());
        }

        public static bool RequiresAuthorization(this IProvideMetadata type) => GetPolicies(type).Any();

        public static Task<AuthorizationResult> Authorize(
            this IProvideMetadata type,
            ClaimsPrincipal principal,
            IDictionary<string, object> userContext,
            Dictionary<string, object> inputVariables,
            IAuthorizationEvaluator evaluator)
        {
            return evaluator.Evaluate(principal, userContext, inputVariables, GetPolicies(type));
        }

        public static void AuthorizeWith(this IProvideMetadata type, params string[] policies)
        {
            if (policies.Length == 0)
                return;

            var list = GetPolicies(type).ToList();
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
            builder.FieldType.AuthorizeWith(policy); // TODO: how does it work?
            return builder;
        }
    }
}
