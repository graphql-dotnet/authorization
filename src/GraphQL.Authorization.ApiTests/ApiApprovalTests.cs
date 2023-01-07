using PublicApiGenerator;

namespace GraphQL.Authorization.ApiTests;

/// <summary>
/// See more info about API approval tests here <see href="https://github.com/JakeGinnivan/ApiApprover"/>.
/// </summary>
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
        publicApi.ShouldMatchApproved(options => options.WithFilenameGenerator((_, _, fileType, fileExtension) => $"{type.Assembly.GetName().Name}.{fileType}.{fileExtension}"));
    }
}
