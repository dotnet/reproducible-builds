using DotNet.ReproducibleBuilds.Tests.Shared;

using FluentAssertions;
using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Isolated.Tests;

public class TargetFrameworkRootPathTests : TestBase
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WarningCanBeSuppressed(bool suppress)
    {
        ProjectCreator.Templates
            .ReproducibleBuildsIsolatedProject(GetRandomFile(".csproj"), configureProject: project =>
            {
                project.Property("TargetFramework", "net472");

                if (suppress)
                {
                    project.Property("NoWarn", "RPB0002");
                }
            })
            // Run the target directly and _not_ as part of Build because we want to test the warning path and not allow the SDK's
            // automatic references to run.
            .TryBuild(restore: false, target: "CheckTargetFrameworkRootPath", out bool result, out BuildOutput output);

        int expected = suppress ? 0 : 1;

        result.Should().BeTrue();
        output.Warnings.Should().HaveCount(expected);
    }
}
