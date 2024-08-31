using FluentAssertions;
using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Tests;

public class MinimumVersionTests : TestBase
{
    [Theory]
    [InlineData("17.7.0", false)]
    [InlineData("17.8.0", true)]
    [InlineData("17.9.0", true)]
    public void BelowMinimumVersionEmitsWarning(string msbuildVersion, bool success)
    {
        Dictionary<string, string> globalProperties = new()
        {
            ["_ReproducibleBuildsMSBuildVersion"] = msbuildVersion
        };

        ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"))
            .TryBuild(restore: false, target: "_ReproducibleBuildsMSBuildVersionCheck", globalProperties, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        output.Warnings.Should().HaveCount(success ? 0 : 1);
    }
}
