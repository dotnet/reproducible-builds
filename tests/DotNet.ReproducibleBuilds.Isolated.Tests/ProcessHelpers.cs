using System.Diagnostics;
using System.Text;

namespace DotNet.ReproducibleBuilds.Isolated.Tests;

internal static class ProcessHelpers
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    public static (int ExitCode, string Stdout, string Stderr) RunAndWaitForExit(ProcessStartInfo psi, TimeSpan? timeout = null)
    {
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.CreateNoWindow = true;

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

        TimeSpan effective = timeout ?? DefaultTimeout;
        if (!process.WaitForExit((int)effective.TotalMilliseconds))
        {
            try
            {
                process.Kill();
            }
            catch
            {
                // Best-effort: the process may have exited between WaitForExit and Kill.
            }

            throw new TimeoutException(
                $"Process '{psi.FileName}' did not exit within {effective}. " +
                $"Captured stdout:{Environment.NewLine}{stdout}{Environment.NewLine}" +
                $"Captured stderr:{Environment.NewLine}{stderr}");
        }

        // Per docs, the parameterless overload ensures the async stdout/stderr handlers
        // have finished draining after the process has exited.
        process.WaitForExit();

        return (process.ExitCode, stdout.ToString(), stderr.ToString());
    }
}
