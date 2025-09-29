using DotNet.ReproducibleBuilds.Tests.Shared;

using FluentAssertions;

using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Isolated.Tests;

public class IsolatedProjectTests : TestBase
{
    [Fact]
    public void ProjectCanRestore()
    {
        ProjectCreator.Templates
            .ReproducibleBuildsIsolatedProject(GetRandomFile(".csproj"), configureProject: project =>
            {
                project.Property("TargetFramework", GetCurrentTargetFrameworkMoniker());
            })
            .TryRestore(out bool result, out BuildOutput output);

        output.Errors.Should().HaveCount(0);
        output.Warnings.Should().HaveCount(0);
        result.Should().BeTrue();
    }
}
