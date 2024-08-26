using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Tests;

internal static class ProjectTemplates
{
    private static readonly string ThisAssemblyDirectory = Path.GetDirectoryName(typeof(ProjectTemplates).Assembly.Location)!;

    public static ProjectCreator ReproducibleBuildProject(this ProjectCreatorTemplates templates, string directory, Action<ProjectCreator> configure)
    {
        ProjectCreator template = ProjectCreator.Templates
            .SdkCsproj(path: Path.Combine(directory, "test.csproj"), targetFramework: "net8.0")
            .Import(Path.Combine(ThisAssemblyDirectory, "DotNet.ReproducibleBuilds.props"));

        configure(template);

        return template
            .Import(Path.Combine(ThisAssemblyDirectory, "DotNet.ReproducibleBuilds.targets"));
    }
}
