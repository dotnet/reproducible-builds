using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Tests;

internal static class ProjectCreatorExtensions
{
    public static Project ProjectWithGlobalProperties(this ProjectCreator creator, IDictionary<string, string> properties)
    {
        creator.TryGetProject(out Project project, properties);

        return project;
    }
}
