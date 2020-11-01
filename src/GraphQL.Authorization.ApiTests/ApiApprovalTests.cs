using System;
using PublicApiGenerator;
using Shouldly;
using Xunit;

namespace GraphQL.Authorization.ApiTests
{
    /// <see href="https://github.com/JakeGinnivan/ApiApprover"/>
    public class ApiApprovalTests
    {
        [Theory]
        [InlineData(typeof(IAuthorizationRequirement))]
        public void public_api_should_not_change_unintentionally(Type type)
        {
            string publicApi = type.Assembly.GeneratePublicApi(new ApiGeneratorOptions
            {
                IncludeAssemblyAttributes = false,
            });

            // See: https://shouldly.readthedocs.io/en/latest/assertions/shouldMatchApproved.html
            // Note: If the AssemblyName.approved.txt file doesn't match the latest publicApi value,
            // this call will try to launch a diff tool to help you out but that can fail on
            // your machine if a diff tool isn't configured/setup. 
            publicApi.ShouldMatchApproved(options => options.WithDiscriminator(type.Assembly.GetName().Name));
        }
    }
}
