using FluentAssertions;
using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Tests;

public class SourceLinkTests : TestBase
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void PublishRepositoryUrlIsSet(bool? publishRepositoryUrl, bool expected)
    {
        ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .PropertyGroup()
                .Property("PublishRepositoryUrl", publishRepositoryUrl.ToLowerInvariant())
            .Project
            .GetPropertyValue("PublishRepositoryUrl")
            .Should().Be(expected.ToLowerInvariant());
    }

    [Theory]
    [InlineData(null, "embedded")]
    [InlineData("embedded", "embedded")]
    [InlineData("portable", "portable")]
    public void DebugTypeIsSet(string? debugType, string expected)
    {
        ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .PropertyGroup()
                .Property("DebugType", debugType)
            .Project
            .GetPropertyValue("DebugType")
            .Should().Be(expected);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void EmbedUntrackedSourcesIsSet(bool? embedUntrackedSources, bool expected)
    {
        ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .PropertyGroup()
                .Property("PublishRepositoryUrl", embedUntrackedSources.ToLowerInvariant())
            .Project
            .GetPropertyValue("PublishRepositoryUrl")
            .Should().Be(expected.ToLowerInvariant());
    }

    [Theory]
    [InlineData("GITHUB_REF", "refs/pull/1234/merge", "pr1234")]
    [InlineData("GITHUB_REF", "refs/heads/my-branch", "my-branch")]
    [InlineData("GITHUB_REF", "refs/tags/v1.2.3", "v1.2.3")]

    [InlineData("BUILD_SOURCEBRANCH", "refs/heads/my-branch", "my-branch")]
    [InlineData("BUILD_SOURCEBRANCH", "refs/tags/v1.2.3", "v1.2.3")]

    [InlineData("APPVEYOR_PULL_REQUEST_NUMBER", "1234", "pr1234")]
    [InlineData("APPVEYOR_REPO_TAG_NAME", "refs/tags/v1.2.3", "refs/tags/v1.2.3")]
    [InlineData("APPVEYOR_REPO_BRANCH", "refs/heads/my-branch", "refs/heads/my-branch")]

    [InlineData("TEAMCITY_BUILD_BRANCH", "refs/heads/my-branch", "refs/heads/my-branch")]

    [InlineData("TRAVIS_PULL_REQUEST", "1234", "pr1234")]
    [InlineData("TRAVIS_BRANCH", "refs/heads/my-branch", "refs/heads/my-branch")]

    [InlineData("CIRCLE_PR_NUMBER", "1234", "pr1234")]
    [InlineData("CIRCLE_TAG", "refs/heads/v1.2.3", "refs/heads/v1.2.3")]
    [InlineData("CIRCLE_BRANCH", "refs/heads/my-branch", "refs/heads/my-branch")]

    [InlineData("CI_COMMIT_TAG", "refs/tags/v1.2.3", "refs/tags/v1.2.3")]
    [InlineData("CI_MERGE_REQUEST_IID", "1234", "pr1234")]
    [InlineData("CI_COMMIT_BRANCH", "refs/heads/my-branch", "refs/heads/my-branch")]

    [InlineData("BUDDY_EXECUTION_PULL_REQUEST_NO", "1234", "pr1234")]
    [InlineData("BUDDY_EXECUTION_TAG", "refs/tags/v1.2.3", "refs/tags/v1.2.3")]
    [InlineData("BUDDY_EXECUTION_BRANCH", "refs/heads/my-branch", "refs/heads/my-branch")]
    public void RepositoryBranchIsSet(string ci, string original, string expected)
    {
        using EnvironmentVariableSuppressor hostSuppressor = new("BUILD_SOURCEBRANCH"); // Suppress our own CI provider variables (i.e. Azure DevOps)
        using EnvironmentVariableSuppressor ciSuppressor = new(ci); // Suppress the mock CI provider (just in case)

        ProjectCreator project = ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .PropertyGroup()
                .Property(ci, original);

        // If RepositoryBranch is not set, it should be set from the CI provider property
        project.Project
            .GetPropertyValue("RepositoryBranch")
            .Should().Be(expected);

        // If RepositoryBranch is set, it should take precedence over the CI provider variables
        project.Property("RepositoryBranch", "explicitly-set")
            .Project
            .GetPropertyValue("RepositoryBranch")
            .Should().Be("explicitly-set", "because explicitly setting `RepositoryBranch` should always win.");
    }
}
