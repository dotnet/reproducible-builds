using Microsoft.Build.Utilities.ProjectCreation;
using System.Runtime.CompilerServices;

namespace DotNet.ReproducibleBuilds.Isolated.Tests;

internal static class MSBuildModuleInitializer
{
    [ModuleInitializer]
    internal static void InitializeMSBuild()
    {
        MSBuildAssemblyResolver.Register();
    }
}
