using DotNet.ReproducibleBuilds.Tests.Shared;

using FluentAssertions;

using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Isolated.Tests;

public class GlobalJsonTests : TestBase
{
    private const string Message = "global.json not found, or does not specify an SDK version. Add one to ensure a consistent build environment.";

    [Fact]
    public void ProjectWithGlobalJsonCanBuild()
    {
        FileInfo project = GetRandomFile(".csproj");

        ProjectCreator.Templates
            .ReproducibleBuildsIsolatedProject(project, configureProject: p =>
            {
                p.Property("TargetFramework", GetCurrentTargetFrameworkMoniker());
            })
            .WithGlobalJson(GetCurrentSdkVersion(), "minor")
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        output.Errors.Should().HaveCount(0);
        output.WarningEvents.Should().BeEmpty();
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProjectWithoutGlobalJsonCanSuppressWarning(bool suppress)
    {
        FileInfo project = GetRandomFile(".csproj");

        ProjectCreator.Templates
            .ReproducibleBuildsIsolatedProject(project, configureProject: p =>
            {
                p.Property("TargetFramework", GetCurrentTargetFrameworkMoniker());
                if (suppress) { p.Property("NoWarn", "RPB0003"); }
            })
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        if (suppress)
        {
            output.WarningEvents.Should().BeEmpty();
            output.ErrorEvents.Should().BeEmpty();

            result.Should().BeTrue();
        }
        else
        {
            output.WarningEvents.Select(e => e.Message).Should().BeEquivalentTo([Message]);
            result.Should().BeTrue();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProjectWithoutGlobalJsonCanElevateToEror(bool warningsAsErrors)
    {
        FileInfo project = GetRandomFile(".csproj");

        ProjectCreator.Templates
            .ReproducibleBuildsIsolatedProject(project, configureProject: p =>
            {
                p.Property("TargetFramework", GetCurrentTargetFrameworkMoniker());
                if (warningsAsErrors) { p.Property("MSBuildTreatWarningsAsErrors", "true"); }
            })
            .TryBuild(restore: true, out bool result, out BuildOutput output);


        if (warningsAsErrors)
        {
            output.ErrorEvents.Select(e => e.Message).Should().BeEquivalentTo([Message]);
            result.Should().BeFalse();
        }
        else
        {
            output.WarningEvents.Select(e => e.Message).Should().BeEquivalentTo([Message]);
            result.Should().BeTrue();
        }
    }
}
