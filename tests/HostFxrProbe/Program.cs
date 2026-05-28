using System;
using System.IO;
using System.Reflection;

namespace HostFxrProbe;

/// <summary>
/// Probe used by <c>HostFxrResolverTests</c> to exercise the hostfxr P/Invoke from a
/// .NET Framework process under controlled environment conditions.
///
/// Loads the net472 build of the <c>DotNet.ReproducibleBuilds.Isolated</c> task assembly,
/// invokes the static <c>hostfxr_set_error_writer</c> P/Invoke via reflection, and reports
/// the outcome via exit code and stderr.
/// </summary>
internal static class Program
{
    private const int ExitSuccess = 0;
    private const int ExitDllNotFound = 2;
    private const int ExitOtherFailure = 3;

    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: HostFxrProbe.exe <path-to-task-dll>");
            return ExitOtherFailure;
        }

        string taskDllPath = args[0];
        if (!File.Exists(taskDllPath))
        {
            Console.Error.WriteLine($"Task DLL not found: {taskDllPath}");
            return ExitOtherFailure;
        }

        // The task DLL is built with ExcludeAssets="Runtime" for Microsoft.Build.Utilities.Core,
        // so its dependencies live in the probe's own bin dir. Help the loader find them.
        string taskDir = Path.GetDirectoryName(taskDllPath)!;
        string probeDir = AppDomain.CurrentDomain.BaseDirectory;
        AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
        {
            string simpleName = new AssemblyName(e.Name).Name;
            string candidate = Path.Combine(probeDir, simpleName + ".dll");
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
}
