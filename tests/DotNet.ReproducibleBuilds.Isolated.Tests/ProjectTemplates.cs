using DotNet.ReproducibleBuilds.Tests.Shared;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Isolated.Tests;

internal static class ProjectTemplates
{
    private static readonly string ThisAssemblyDirectory = Path.GetDirectoryName(typeof(ProjectTemplates).Assembly.Location)!;

    public static ProjectCreator ReproducibleBuildsIsolatedProject(this ProjectCreatorTemplates templates, FileInfo project, Action<ProjectCreator>? configureProject = null)
    {
        DirectoryInfo directory = project.Directory ?? throw new ArgumentException("Project's path does not appear to have a parent.", nameof(project));

        ProjectCollection projectCollection = new(); // Create a new collection for each project to ensure environment variables aren't shared between tests

        return ProjectCreator
            .Create(path: project.FullName, sdk: ProjectCreatorConstants.SdkCsprojDefaultSdk, projectCollection: projectCollection)
            .Import(Path.Combine(ThisAssemblyDirectory, "Sdk.props"))
            .CustomAction(configureProject)
            .Import(Path.Combine(ThisAssemblyDirectory, "Sdk.targets"));
    }
}