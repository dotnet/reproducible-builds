using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Tests;

internal static class ProjectTemplates
{
    private static readonly string ThisAssemblyDirectory = Path.GetDirectoryName(typeof(ProjectTemplates).Assembly.Location)!;

    public static ProjectCreator ReproducibleBuildProject(this ProjectCreatorTemplates templates, FileInfo project)
    {
        DirectoryInfo directory = project.Directory ?? throw new ArgumentException("Project's path does not appear to have a parent.", nameof(project));

        _ = ProjectCreator
            .Create(path: directory.Combine("obj", $"{project.Name}.tests.g.props"))
            .Import(Path.Combine(ThisAssemblyDirectory, "DotNet.ReproducibleBuilds.props"))
            .Save();

        _ = ProjectCreator
            .Create(path: directory.Combine("obj", $"{project.Name}.tests.g.targets"))
            .Import(Path.Combine(ThisAssemblyDirectory, "DotNet.ReproducibleBuilds.targets"))
            .Save();

        return templates
            .SdkCsproj(path: project.FullName, targetFramework: "net8.0");
    }
}
