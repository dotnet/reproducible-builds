using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace StubTaskHarness;

/// <summary>
/// Stub MSBuild host used by <c>HostFxrResolverTests</c> to exercise the hostfxr P/Invoke from a
/// .NET Framework process under controlled environment conditions. It is the minimal stand-in for
/// an MSBuild host: it loads <c>Microsoft.Build.Utilities.Core</c> (just enough for the task type
/// to resolve), <c>Assembly.LoadFrom</c>s the net472 build of the
/// <c>DotNet.ReproducibleBuilds.Isolated</c> task assembly, invokes the static
/// <c>hostfxr_set_error_writer</c> P/Invoke via reflection, and reports the outcome via exit code
/// and stderr. It does not load MSBuild's build engine (no <c>BuildManager</c>, no targets
/// execution).
/// </summary>
internal static class Program
{
    private const int ExitSuccess = 0;
    private const int ExitHostFxrAlreadyLoaded = 1;
    private const int ExitDllNotFound = 2;
    private const int ExitOtherFailure = 3;

    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: StubTaskHarness.exe <path-to-task-dll>");
            return ExitOtherFailure;
        }

        string taskDllPath = args[0];
        if (!File.Exists(taskDllPath))
        {
            Console.Error.WriteLine($"Task DLL not found: {taskDllPath}");
            return ExitOtherFailure;
        }

        // Defend against the test going green via an already-loaded hostfxr in this process.
        // If something else loaded hostfxr before the task's P/Invoke runs, Windows would
        // satisfy the [DllImport("hostfxr")] from the already-loaded module and HostFxrResolver
        // would never be exercised.
        if (IsHostFxrLoaded())
        {
            Console.Error.WriteLine("PRECONDITION FAILED: hostfxr.dll is already loaded in this process; the test cannot prove HostFxrResolver did anything.");
            return ExitHostFxrAlreadyLoaded;
        }

        // The task DLL is built with ExcludeAssets="Runtime" for Microsoft.Build.Utilities.Core,
        // so its dependencies live in the harness's own bin dir. Help the loader find them.
        string taskDir = Path.GetDirectoryName(taskDllPath)!;
        string harnessDir = AppDomain.CurrentDomain.BaseDirectory;
        AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
        {
            string simpleName = new AssemblyName(e.Name).Name;
            string candidate = Path.Combine(harnessDir, simpleName + ".dll");
            if (File.Exists(candidate))
            {
                return Assembly.LoadFrom(candidate);
            }
            candidate = Path.Combine(taskDir, simpleName + ".dll");
            return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
        };

        try
        {
            Assembly taskAsm = Assembly.LoadFrom(taskDllPath);

            // Second precondition: loading the task assembly (and any transitive deps the loader
            // pulls in) must not have mapped hostfxr.dll into the process either. If it did, the
            // P/Invoke below would bind to the pre-loaded module and bypass HostFxrResolver
            // entirely - the test would go green without exercising the production code.
            if (IsHostFxrLoaded())
            {
                Console.Error.WriteLine("PRECONDITION FAILED: hostfxr.dll was loaded as a side effect of Assembly.LoadFrom; the test cannot prove HostFxrResolver did anything.");
                return ExitHostFxrAlreadyLoaded;
            }

            Type taskType = taskAsm.GetType("DotNet.ReproducibleBuilds.Isolated.ValidateGlobalJsonSdkVersion", throwOnError: true)!;
            MethodInfo setErrorWriter = taskType.GetMethod(
                "hostfxr_set_error_writer",
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("hostfxr_set_error_writer not found on ValidateGlobalJsonSdkVersion.");

            // Invoking the static method runs the type's static ctor, which calls
            // HostFxrResolver.Register(). Then the P/Invoke fires hostfxr's LoadLibrary.
            object? previous = setErrorWriter.Invoke(null, new object[] { IntPtr.Zero });
            Console.Out.WriteLine($"hostfxr_set_error_writer returned: 0x{((IntPtr)previous!).ToInt64():X}");
            return ExitSuccess;
        }
        catch (TargetInvocationException tie) when (tie.InnerException is DllNotFoundException dnf)
        {
            Console.Error.WriteLine($"REPRODUCED: {dnf.GetType().FullName}: {dnf.Message}");
            return ExitDllNotFound;
        }
        catch (TypeInitializationException tie) when (FindInner<DllNotFoundException>(tie) is { } dnf)
        {
            Console.Error.WriteLine($"REPRODUCED (TypeInitializationException): {dnf.GetType().FullName}: {dnf.Message}");
            return ExitDllNotFound;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"UNEXPECTED: {ex.GetType().FullName}: {ex.Message}");
            Console.Error.WriteLine(ex.ToString());
            return ExitOtherFailure;
        }
    }

    private static bool IsHostFxrLoaded() => GetModuleHandleW("hostfxr.dll") != IntPtr.Zero;

    private static T? FindInner<T>(Exception ex) where T : Exception
    {
        for (Exception? e = ex; e is not null; e = e.InnerException)
        {
            if (e is T t)
            {
                return t;
            }
        }
        return null;
    }

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    private static extern IntPtr GetModuleHandleW(string lpModuleName);
}

