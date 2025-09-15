using DotNet.ReproducibleBuilds.Tests.Shared;

using FluentAssertions;

using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Isolated.Tests;

public class IsolatedProjectTests : TestBase
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void ProjectCanRestore(bool hasGlobalJson, bool suppress)
    {
        FileInfo project = GetRandomFile(".csproj");

        if (hasGlobalJson)
        {
            File.WriteAllText(Path.Combine(project.DirectoryName!, "global.json"),
            $$"""
            {
              "sdk": {
                "version": "{{GetCurrentSdkVersion()}}",
                "rollForward": "minor"
              }
            }
            """);
        }

        ProjectCreator.Templates
            .ReproducibleBuildsIsolatedProject(project, configureProject: project =>
            {
                project.Property("TargetFramework", GetCurrentTargetFrameworkMoniker());

                if (suppress)
                {
                    project.Property("NoWarn", "RPB0003");
                }
            })
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        output.Errors.Should().HaveCount(0);
        result.Should().BeTrue();

        string[] expectedWarnings = (hasGlobalJson || suppress) ? [] : ["global.json not found, or does not specify an SDK version. Add one to ensure a consistent build environment."];
        output.WarningEvents.Select(e => e.Message).Should().BeEquivalentTo(expectedWarnings);
    }
}
