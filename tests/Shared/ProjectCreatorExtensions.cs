using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities.ProjectCreation;

namespace DotNet.ReproducibleBuilds.Tests.Shared;

internal static class ProjectCreatorExtensions
{
    public static Project ProjectWithGlobalProperties(this ProjectCreator creator, IDictionary<string, string> properties)
    {
        creator.TryGetProject(out Project project, properties);

        return project;
    }

    public static ProjectCreator Properties(this ProjectCreator creator, IEnumerable<KeyValuePair<string, string?>> properties)
    {
        foreach (KeyValuePair<string, string?> property in properties)
        {
            creator.Property(property.Key, property.Value);
        }

        return creator;
    }
}
