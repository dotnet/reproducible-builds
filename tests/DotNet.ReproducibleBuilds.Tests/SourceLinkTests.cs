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
            .ReproducibleBuildProject(TestRootPath)
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
            .ReproducibleBuildProject(TestRootPath)
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
            .ReproducibleBuildProject(TestRootPath)
            .PropertyGroup()
                .Property("PublishRepositoryUrl", embedUntrackedSources.ToLowerInvariant())
            .Project
            .GetPropertyValue("PublishRepositoryUrl")
            .Should().Be(expected.ToLowerInvariant());
    }
}
