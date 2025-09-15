using FluentAssertions;
using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Tests;

public class MinimumVersionTests : TestBase
{
    [Theory]
    [InlineData("17.7.0", false, false)]
    [InlineData("17.7.0", false, true)]
    [InlineData("17.8.0", true, false)]
    [InlineData("17.9.0", true, true)]
    public void BelowMinimumVersionEmitsWarning(string msbuildVersion, bool success, bool suppress)
    {
        Dictionary<string, string> globalProperties = new()
        {
            ["_ReproducibleBuildsMSBuildVersion"] = msbuildVersion
        };

        ProjectCreator project = ProjectCreator.Templates
            .ReproducibleBuildProject(GetRandomFile(".csproj"));

        if (suppress)
        {
            project.Property("NoWarn", "RPB0001"); // Suppress the RPB0001 warning
        }

        project.TryBuild(restore: false, target: "_ReproducibleBuildsMSBuildVersionCheck", globalProperties, out bool result, out BuildOutput output);

        int expected = (success || suppress) ? 0 : 1;

        result.Should().BeTrue();
        output.Warnings.Should().HaveCount(expected);
    }
}
