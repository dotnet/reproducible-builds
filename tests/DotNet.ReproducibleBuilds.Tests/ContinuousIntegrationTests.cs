using FluentAssertions;
using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Tests;

public class ContinuousIntegrationTests : TestBase
{
    private const string ContinuousIntegrationBuild = "ContinuousIntegrationBuild";

    [Theory]
    [InlineData(null, "")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void RespectsSetValue(bool? value, string expected)
    {
        using EnvironmentVariableSuppressor hostSuppressor = new("TF_BUILD"); // Suppress our own CI provider variables (i.e. Azure DevOps)

        ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .PropertyGroup()
                .Property(ContinuousIntegrationBuild, value?.ToLowerInvariant())
            .Project
            .GetPropertyValue(ContinuousIntegrationBuild)
            .Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(MemberData))]
    public void RespectsGlobalProperties(Dictionary<string, string> envVars)
    {
        using EnvironmentVariableSuppressor hostSuppressor = new("TF_BUILD"); // Suppress our own CI provider variables (i.e. Azure DevOps)

        // If ContinuousIntegrationBuild is not set, it should be set from the CI provider property
        ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .ProjectWithGlobalProperties(envVars)
            .GetPropertyValue(ContinuousIntegrationBuild)
            .Should().Be(true.ToLowerInvariant());

        // If ContinuousIntegrationBuild is set, it should take precedence over the CI provider variables
        ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .ProjectWithGlobalProperties(envVars.With(ContinuousIntegrationBuild, false.ToLowerInvariant()))
            .GetPropertyValue(ContinuousIntegrationBuild)
            .Should().Be(false.ToLowerInvariant(), "because explicitly setting `ContinuousIntegrationBuild` should always win.");
    }

    public static TheoryData<Dictionary<string, string>> MemberData()
    {
        return
        [
            new TheoryDataRow<Dictionary<string, string>>(new() {  ["TF_BUILD"] = "True" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["GITHUB_ACTIONS"] = "true" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["APPVEYOR"] = "True" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["CI"] = "true" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["TRAVIS"] = "true" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["CIRCLECI"] = "true" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["CODEBUILD_BUILD_ID"] = "abc:123", ["AWS_REGION"] = "us-east-1" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["BUILD_ID"] = "123", ["BUILD_URL"] = "https://buildserver.invalid/jenkins/job/MyJobName/123/" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["BUILD_ID"] = "123", ["PROJECT_ID"] = "234" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["TEAMCITY_VERSION"] = "10" }),
            new TheoryDataRow<Dictionary<string, string>>(new() { ["JB_SPACE_API_URL"] = "https://api.invalid/url" }),
        ];
    }
}
