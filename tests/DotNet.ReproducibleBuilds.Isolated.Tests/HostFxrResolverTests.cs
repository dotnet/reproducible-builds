using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using FluentAssertions;

namespace DotNet.ReproducibleBuilds.Isolated.Tests;

/// <summary>
/// Validates that the net472 build of the task can resolve the <c>hostfxr</c> native library
/// when MSBuild hosts it on .NET Framework (e.g. Visual Studio's in-process build).
///
/// Regression test for https://github.com/dotnet/reproducible-builds/issues/79: in 2.0.2
/// the <c>HostFxrResolver</c> was compiled only into the <c>net6.0</c> build, so the
/// <c>net472</c> task fell through to Windows' default DLL search and threw
/// <see cref="DllNotFoundException"/> unless an unrelated process had already loaded
/// <c>hostfxr.dll</c>.
/// </summary>
public class HostFxrResolverTests
{
    [Fact]
    public void NetFrameworkTask_ResolvesHostFxr_WhenNoCopyIsDirectlyOnPath()
    {
        Assert.SkipUnless(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            "Issue #79 is specific to Visual Studio hosting MSBuild on .NET Framework, which only happens on Windows.");

        string testBin = AppContext.BaseDirectory;
        string harnessExe = Path.Combine(testBin, "harness", "StubTaskHarness.exe");
        string taskDll = Path.Combine(testBin, "tasks", "net472", "DotNet.ReproducibleBuilds.Isolated.dll");

        File.Exists(harnessExe).Should().BeTrue($"the harness must be copied into the test bin (looked at {harnessExe})");
        File.Exists(taskDll).Should().BeTrue($"the net472 task DLL must be copied into the test bin (looked at {taskDll})");

        string dotnetDir = FindDotnetExeDirectory();
        string systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string path = string.Join(";", dotnetDir, system32, systemRoot);

        // Guard against the test accidentally going green: hostfxr.dll must not be discoverable
        // through Windows' default DLL search, otherwise the P/Invoke would succeed even without
        // the resolver. The resolver must reach `<dotnet>\host\fxr\<ver>\hostfxr.dll` for the
        // load to succeed.
        AssertNoHostFxrIn(Path.GetDirectoryName(harnessExe)!);
        AssertNoHostFxrIn(Path.GetDirectoryName(taskDll)!);
        AssertNoHostFxrIn(dotnetDir);
        AssertNoHostFxrIn(system32);
        AssertNoHostFxrIn(systemRoot);

        var psi = new ProcessStartInfo(harnessExe, $"\"{taskDll}\"")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = systemRoot,
        };

        // Build a controlled, minimal environment that mirrors the reporter's scenario:
        // - PATH has `dotnet.exe` (so HostFxrResolver can locate dotnet via PATH) and the system dirs,
        //   but no directory that contains `hostfxr.dll` directly (e.g. PowerShell 7's copy is excluded).
        // - All DOTNET_* discovery shortcuts are cleared so the test exercises the PATH-based
        //   probing path that VS-hosted MSBuild typically uses.
        psi.EnvironmentVariables["PATH"] = path;
        psi.EnvironmentVariables.Remove("DOTNET_ROOT");
        psi.EnvironmentVariables.Remove("DOTNET_ROOT(x86)");
        psi.EnvironmentVariables.Remove("DOTNET_ROOT(arm64)");
        psi.EnvironmentVariables.Remove("DOTNET_HOST_PATH");
        psi.EnvironmentVariables.Remove("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR");

        // Use async OutputDataReceived/ErrorDataReceived rather than synchronous ReadToEnd calls so
        // a chatty harness can't deadlock by filling either pipe's OS buffer while we're blocked on
        // the other stream.
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        using var process = new Process { StartInfo = psi };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                stdout.AppendLine(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                stderr.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        process.ExitCode.Should().Be(
            0,
            $"the net472 task must resolve hostfxr through HostFxrResolver.{Environment.NewLine}" +
            $"stdout: {stdout}{Environment.NewLine}" +
            $"stderr: {stderr}");
    }

    private static void AssertNoHostFxrIn(string directory)
    {
        string candidate = Path.Combine(directory, "hostfxr.dll");
        File.Exists(candidate).Should().BeFalse(
            $"hostfxr.dll at '{candidate}' would let Windows' default DLL search satisfy the P/Invoke, " +
            "which would make this test pass even without a working HostFxrResolver.");
    }

    private static string FindDotnetExeDirectory()
    {
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            foreach (string dir in pathEnv!.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(dir))
                {
                    continue;
                }
                string candidate = Path.Combine(dir, "dotnet.exe");
                if (File.Exists(candidate))
                {
                    return Path.GetDirectoryName(candidate)!;
                }
            }
        }

        throw new InvalidOperationException(
            "Environmental precondition not met: dotnet.exe is not on PATH. " +
            "HostFxrResolver requires PATH-based discovery to find the .NET install. " +
            "Install the .NET SDK and ensure dotnet.exe is on PATH before running this test.");
    }
}
