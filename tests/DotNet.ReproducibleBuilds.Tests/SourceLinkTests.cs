using DotNet.ReproducibleBuilds.Tests.Shared;

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
    [MemberData(nameof(RepositoryBranchData))]
    public void RepositoryBranchIsSet(Dictionary<string, string?> env, string expected)
    {
        using EnvironmentVariableSuppressor hostSuppressor = new("BUILD_SOURCEBRANCH"); // Suppress our own CI provider variables (i.e. Azure DevOps)
        using IDisposable ciSuppressors = env.Select(kvp => new EnvironmentVariableSuppressor(kvp.Key)).ToDisposable(); // Suppress the mock CI provider (just in case)

        ProjectCreator project = ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .PropertyGroup()
                .Properties(env);

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

    public static TheoryData<Dictionary<string, string?>, string> RepositoryBranchData()
    {
        TheoryData<Dictionary<string, string?>, string> data = new()
        {
            { new() { ["GITHUB_REF"] = "refs/pull/1234/merge" }, "refs/pull/1234/merge" },
            { new() { ["GITHUB_REF"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["GITHUB_REF"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },

            { new() { ["BUILD_SOURCEBRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["BUILD_SOURCEBRANCH"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },

            { new() { ["APPVEYOR_PULL_REQUEST_NUMBER"] = "1234" }, "pr1234" },
            { new() { ["APPVEYOR_REPO_TAG_NAME"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },
            { new() { ["APPVEYOR_REPO_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["APPVEYOR_PULL_REQUEST_NUMBER"] = "1234" , ["APPVEYOR_REPO_BRANCH"] = "refs/heads/my-branch" }, "pr1234" },
            { new() { ["APPVEYOR_REPO_TAG_NAME"] = "refs/tags/v1.2.3" , ["APPVEYOR_REPO_BRANCH"] = "refs/heads/my-branch" }, "refs/tags/v1.2.3" },
            { new() { ["APPVEYOR_PULL_REQUEST_NUMBER"] = "1234", ["APPVEYOR_REPO_TAG_NAME"] = "refs/tags/v1.2.3", ["APPVEYOR_REPO_BRANCH"] = "refs/heads/my-branch" }, "pr1234" },

            { new() { ["TEAMCITY_BUILD_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },

            { new() { ["TRAVIS_PULL_REQUEST"] = "1234" }, "pr1234" },
            { new() { ["TRAVIS_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["TRAVIS_PULL_REQUEST"] = "1234", ["TRAVIS_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["TRAVIS_PULL_REQUEST"] = "false", ["TRAVIS_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },

            { new() { ["CIRCLE_PR_NUMBER"] = "1234" }, "pr1234" },
            { new() { ["CIRCLE_TAG"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },
            { new() { ["CIRCLE_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["CIRCLE_PR_NUMBER"] = "1234", ["CIRCLE_TAG"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },
            { new() { ["CIRCLE_PR_NUMBER"] = "1234", ["CIRCLE_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["CIRCLE_TAG"] = "refs/tags/v1.2.3", ["CIRCLE_BRANCH"] = "refs/heads/my-branch" }, "refs/tags/v1.2.3" },
            { new() { ["CIRCLE_PR_NUMBER"] = "1234", ["CIRCLE_TAG"] = "refs/tags/v1.2.3", ["CIRCLE_BRANCH"] = "refs/heads/my-branch" }, "refs/tags/v1.2.3" },

            { new() { ["CI_COMMIT_TAG"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },
            { new() { ["CI_MERGE_REQUEST_IID"] = "1234" }, "pr1234" },
            { new() { ["CI_EXTERNAL_PULL_REQUEST_IID"] = "5678" }, "pr5678" },
            { new() { ["CI_COMMIT_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["CI_COMMIT_TAG"] = "refs/tags/v1.2.3", ["CI_MERGE_REQUEST_IID"] = "1234" }, "refs/tags/v1.2.3" },
            { new() { ["CI_COMMIT_TAG"] = "refs/tags/v1.2.3", ["CI_EXTERNAL_PULL_REQUEST_IID"] = "5678" }, "refs/tags/v1.2.3" },
            { new() { ["CI_COMMIT_TAG"] = "refs/tags/v1.2.3", ["CI_COMMIT_BRANCH"] = "refs/heads/my-branch" }, "refs/tags/v1.2.3" },
            { new() { ["CI_MERGE_REQUEST_IID"] = "1234", ["CI_EXTERNAL_PULL_REQUEST_IID"] = "5678" }, "pr1234" },
            { new() { ["CI_MERGE_REQUEST_IID"] = "1234", ["CI_COMMIT_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["CI_EXTERNAL_PULL_REQUEST_IID"] = "5678", ["CI_COMMIT_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["CI_COMMIT_TAG"] = "refs/tags/v1.2.3", ["CI_MERGE_REQUEST_IID"] = "1234", ["CI_EXTERNAL_PULL_REQUEST_IID"] = "5678" }, "refs/tags/v1.2.3" },
            { new() { ["CI_COMMIT_TAG"] = "refs/tags/v1.2.3", ["CI_MERGE_REQUEST_IID"] = "1234", ["CI_COMMIT_BRANCH"] = "refs/heads/my-branch" }, "refs/tags/v1.2.3" },
            { new() { ["CI_COMMIT_TAG"] = "refs/tags/v1.2.3", ["CI_EXTERNAL_PULL_REQUEST_IID"] = "5678", ["CI_COMMIT_BRANCH"] = "refs/heads/my-branch" }, "refs/tags/v1.2.3" },
            { new() { ["CI_MERGE_REQUEST_IID"] = "1234", ["CI_EXTERNAL_PULL_REQUEST_IID"] = "5678", ["CI_COMMIT_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["CI_COMMIT_TAG"] = "refs/tags/v1.2.3", ["CI_MERGE_REQUEST_IID"] = "1234", ["CI_EXTERNAL_PULL_REQUEST_IID"] = "5678", ["CI_COMMIT_BRANCH"] = "refs/heads/my-branch" }, "refs/tags/v1.2.3" },

            { new() { ["BUDDY_EXECUTION_PULL_REQUEST_NO"] = "1234" }, "pr1234" },
            { new() { ["BUDDY_EXECUTION_TAG"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },
            { new() { ["BUDDY_EXECUTION_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["BUDDY_EXECUTION_PULL_REQUEST_NO"] = "1234", ["BUDDY_EXECUTION_TAG"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },
            { new() { ["BUDDY_EXECUTION_PULL_REQUEST_NO"] = "1234", ["BUDDY_EXECUTION_BRANCH"] = "refs/heads/my-branch" }, "refs/heads/my-branch" },
            { new() { ["BUDDY_EXECUTION_TAG"] = "refs/tags/v1.2.3", ["BUDDY_EXECUTION_BRANCH"] = "refs/heads/my-branch" }, "refs/tags/v1.2.3" },
            { new() { ["BUDDY_EXECUTION_PULL_REQUEST_NO"] = "1234", ["BUDDY_EXECUTION_BRANCH"] = "refs/heads/my-branch", ["BUDDY_EXECUTION_TAG"] = "refs/tags/v1.2.3" }, "refs/tags/v1.2.3" },
        };

        return data;
    }
}
