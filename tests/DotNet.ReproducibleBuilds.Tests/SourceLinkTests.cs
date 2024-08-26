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
        ProjectCreator.Templates.ReproducibleBuildProject(TestRootPath, project =>
        {
            project
                .PropertyGroup()
                .Property("PublishRepositoryUrl", publishRepositoryUrl.ToLowerInvariant());
        }).Project
            .GetPropertyValue("PublishRepositoryUrl")
            .Should().Be(expected.ToLowerInvariant());
    }
}
