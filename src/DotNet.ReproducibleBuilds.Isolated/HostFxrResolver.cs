#if NET
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace DotNet.ReproducibleBuilds.Isolated;

/// <summary>
/// Provides custom resolution for the hostfxr native library.
/// Required for Alpine Linux and other environments where hostfxr is not in the default search paths.
/// Uses AssemblyLoadContext.ResolvingUnmanagedDll event for resolution.
/// </summary>
/// <remarks>
/// Based on https://github.com/microsoft/MSBuildLocator/blob/c16c5354e9fda9703933079528ae67bb5ae4e34e/src/MSBuildLocator/DotNetSdkLocationHelper.cs
/// </remarks>
internal static class HostFxrResolver
{
    private static readonly object s_lock = new();
    private static bool s_resolverAdded;
    private static readonly Lazy<List<string>> s_dotnetPathCandidates = new(ResolveDotnetPathCandidates);

    internal const string HostFxrName = "hostfxr";
    private static string ExeName => OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";

    /// <summary>
    /// Registers a resolver for hostfxr with the current AssemblyLoadContext.
    /// This enables hostfxr to be found on Alpine Linux and other environments
    /// where it's not in the default search paths.
    /// </summary>
    public static void Register()
    {
        lock (s_lock)
        {
            if (s_resolverAdded)
            {
                return;
            }

            s_resolverAdded = true;

            // For Windows hostfxr is loaded in the process.
            if (OperatingSystem.IsWindows())
            {
                return;
            }

            var loadContext = AssemblyLoadContext.GetLoadContext(typeof(HostFxrResolver).Assembly);
            if (loadContext != null)
            {
                loadContext.ResolvingUnmanagedDll += HostFxrResolver_ResolvingUnmanagedDll;
            }
        }
    }

    private static IntPtr HostFxrResolver_ResolvingUnmanagedDll(Assembly assembly, string libraryName)
    {
        // The DllImport hardcoded the name as hostfxr.
        if (!libraryName.Equals(HostFxrName, StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        string hostFxrLibName = OperatingSystem.IsWindows()
            ? $"{HostFxrName}.dll"
            : OperatingSystem.IsMacOS()
                ? $"lib{HostFxrName}.dylib"
                : $"lib{HostFxrName}.so";

        foreach (string dotnetPath in s_dotnetPathCandidates.Value)
        {
            string hostFxrRoot = Path.Combine(dotnetPath, "host", "fxr");
            if (!Directory.Exists(hostFxrRoot))
            {
                continue;
            }

            // Get version directories and sort descending (newest first)
            string[] versionDirs;
            try
            {
                versionDirs = Directory.GetDirectories(hostFxrRoot);
            }
            catch
            {
                continue;
            }

            Array.Sort(versionDirs, (a, b) =>
            {
                string versionA = Path.GetFileName(a);
                string versionB = Path.GetFileName(b);
                if (Version.TryParse(versionA.Split('-')[0], out Version? va) &&
                    Version.TryParse(versionB.Split('-')[0], out Version? vb))
                {
                    return vb.CompareTo(va);
                }
                return string.Compare(versionB, versionA, StringComparison.OrdinalIgnoreCase);
            });

            foreach (string versionDir in versionDirs)
            {
                string hostFxrAssembly = Path.Combine(versionDir, hostFxrLibName);
                if (NativeLibrary.TryLoad(hostFxrAssembly, out IntPtr handle))
                {
                    return handle;
                }
            }
        }

        return IntPtr.Zero;
    }

    private static List<string> ResolveDotnetPathCandidates()
    {
        var pathCandidates = new List<string>();

        // DOTNET_ROOT (architecture-specific on 32-bit)
        AddIfValid(GetDotnetPathFromROOT());

        // DOTNET_HOST_PATH
        string? hostPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (!string.IsNullOrEmpty(hostPath) && File.Exists(hostPath))
        {
            AddIfValid(ValidatePath(Path.GetDirectoryName(hostPath)));
        }

        // DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR
        AddIfValid(FindDotnetPathFromEnvVariable("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR"));

        // PATH
        AddIfValid(GetDotnetPathFromPATH());

        return pathCandidates;

        void AddIfValid(string? path)
        {
            if (!string.IsNullOrEmpty(path) && !pathCandidates.Contains(path!))
            {
                pathCandidates.Add(path!);
            }
        }
    }

    private static string? GetDotnetPathFromROOT()
    {
        // 32-bit architecture has (x86) suffix
        string envVarName = IntPtr.Size == 4 ? "DOTNET_ROOT(x86)" : "DOTNET_ROOT";
        return FindDotnetPathFromEnvVariable(envVarName);
    }

    private static string? GetDotnetPathFromPATH()
    {
        // We will generally find the dotnet exe on the path, but on linux, it is often just a 'dotnet' symlink
        // (possibly even to more symlinks) that we have to resolve to the real dotnet executable.
        string[] paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        foreach (string dir in paths)
        {
            string? filePath = ValidatePath(dir);
            if (!string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }
        }

        return null;
    }

    private static string? FindDotnetPathFromEnvVariable(string environmentVariable)
    {
        string? dotnetPath = Environment.GetEnvironmentVariable(environmentVariable);
        return string.IsNullOrEmpty(dotnetPath) ? null : ValidatePath(dotnetPath);
    }

    private static string? ValidatePath(string? dotnetPath)
    {
        if (string.IsNullOrEmpty(dotnetPath))
        {
            return null;
        }

        string fullPathToDotnetFromRoot = Path.Combine(dotnetPath!, ExeName);
        if (File.Exists(fullPathToDotnetFromRoot))
        {
            if (!OperatingSystem.IsWindows())
            {
                string? resolved = File.ResolveLinkTarget(fullPathToDotnetFromRoot, returnFinalTarget: true)?.FullName;
                if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
                {
                    return Path.GetDirectoryName(resolved);
                }
            }

            return dotnetPath;
        }

        return null;
    }
}
#endif
