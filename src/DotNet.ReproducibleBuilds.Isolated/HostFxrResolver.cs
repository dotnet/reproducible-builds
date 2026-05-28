using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Loader;
#endif

namespace DotNet.ReproducibleBuilds.Isolated;

/// <summary>
/// Provides custom resolution for the hostfxr native library.
/// Required for Alpine Linux and other environments where hostfxr is not in the default search paths.
/// </summary>
/// <remarks>
/// Based on https://github.com/microsoft/MSBuildLocator/blob/c16c5354e9fda9703933079528ae67bb5ae4e34e/src/MSBuildLocator/DotNetSdkLocationHelper.cs.
///
/// On .NET, this hooks <see cref="AssemblyLoadContext.ResolvingUnmanagedDll"/> so hostfxr is located
/// when the first P/Invoke fails the default loader search. The canonical case is Alpine Linux, where
/// <c>libhostfxr.so</c> lives at <c>{dotnet-root}/host/fxr/{version}/</c> and the loader
/// doesn't probe that path on its own.
///
/// On .NET Framework, AssemblyLoadContext is not available and the default Windows DLL search order will
/// never find hostfxr (it lives in a versioned subdirectory of the dotnet install, not on PATH). To make
/// the subsequent <c>[DllImport("hostfxr")]</c> bind, this class eagerly calls <c>LoadLibraryW</c> on the
/// full path of the highest-version hostfxr it can find.
/// </remarks>
internal static class HostFxrResolver
{
    private static readonly object s_lock = new();
    private static bool s_resolverAdded;
    private static readonly Lazy<List<string>> s_dotnetPathCandidates = new(ResolveDotnetPathCandidates);

    internal const string HostFxrName = "hostfxr";

    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private static string ExeName => IsWindows ? "dotnet.exe" : "dotnet";

    /// <summary>
    /// Registers a resolver for hostfxr so that subsequent P/Invokes can find it.
    /// </summary>
    public static void Register()
    {
        lock (s_lock)
        {
            if (s_resolverAdded)
            {
                return;
            }

#if NET
            // On .NET, this assembly is loaded into a process that's almost certainly hosted by
            // dotnet.exe (or a custom apphost), which means hostfxr is already mapped into the
            // process and the P/Invoke binds without any help. Skip the resolver in that case.
            // Custom .NET hosts that don't preload hostfxr exist but aren't a real scenario for an
            // MSBuild task; the AssemblyLoadContext fallback below covers the non-Windows case.
            if (IsWindows)
            {
                s_resolverAdded = true;
                return;
            }

            var loadContext = AssemblyLoadContext.GetLoadContext(typeof(HostFxrResolver).Assembly);
            if (loadContext != null)
            {
                loadContext.ResolvingUnmanagedDll += HostFxrResolver_ResolvingUnmanagedDll;
            }

            s_resolverAdded = true;
#else
            // On .NET Framework the task runs inside an MSBuild process that does not preload hostfxr.
            // The default Windows DLL search order can't locate it because hostfxr.dll lives in
            // `<dotnet-root>\host\fxr\<version>\hostfxr.dll`, which is not on PATH. Eagerly LoadLibrary
            // the highest-version copy so the [DllImport("hostfxr")] in ValidateGlobalJsonSdkVersion
            // binds to the already-loaded module.
            //
            // This branch is Windows-only - net472 outside Windows isn't a real scenario for this task,
            // and LoadLibraryW lives in kernel32.
            if (!IsWindows)
            {
                s_resolverAdded = true;
                return;
            }

            if (TryEagerLoadHostFxr())
            {
                s_resolverAdded = true;
            }
#endif
        }
    }

#if NET
    private static IntPtr HostFxrResolver_ResolvingUnmanagedDll(Assembly assembly, string libraryName)
    {
        // The DllImport hardcoded the name as hostfxr.
        if (!libraryName.Equals(HostFxrName, StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        foreach (string hostFxrAssembly in EnumerateHostFxrCandidates())
        {
            if (NativeLibrary.TryLoad(hostFxrAssembly, out IntPtr handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }
#else
    private static bool TryEagerLoadHostFxr()
    {
        foreach (string hostFxrAssembly in EnumerateHostFxrCandidates())
        {
            if (LoadLibraryW(hostFxrAssembly) != IntPtr.Zero)
            {
                return true;
            }
        }

        return false;
    }

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr LoadLibraryW(string lpFileName);
#endif

    /// <summary>
    /// Enumerates full paths to candidate hostfxr native libraries across all known dotnet install
    /// locations, ordered from highest version to lowest within each install root.
    /// </summary>
    private static IEnumerable<string> EnumerateHostFxrCandidates()
    {
        string hostFxrLibName = IsWindows
            ? $"{HostFxrName}.dll"
            : IsMacOS
                ? $"lib{HostFxrName}.dylib"
                : $"lib{HostFxrName}.so";

        foreach (string dotnetPath in s_dotnetPathCandidates.Value)
        {
            string hostFxrRoot = Path.Combine(dotnetPath, "host", "fxr");
            if (!Directory.Exists(hostFxrRoot))
            {
                continue;
            }

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
                yield return Path.Combine(versionDir, hostFxrLibName);
            }
        }
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
#if NET
            if (!IsWindows)
            {
                string? resolved = File.ResolveLinkTarget(fullPathToDotnetFromRoot, returnFinalTarget: true)?.FullName;
                if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
                {
                    return Path.GetDirectoryName(resolved);
                }
            }
#endif

            return dotnetPath;
        }

        return null;
    }
}

