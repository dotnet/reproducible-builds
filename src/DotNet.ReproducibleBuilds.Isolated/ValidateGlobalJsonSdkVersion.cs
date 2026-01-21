using System;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DotNet.ReproducibleBuilds.Isolated;

/// <summary>
/// MSBuild task that validates a global.json file exists and specifies an SDK version.
/// Uses the hostfxr native library to resolve SDK information.
/// </summary>
/// <remarks>
/// Based on https://github.com/microsoft/MSBuildLocator/blob/0bbf4988996176904ecc0dabb73165919fac605f/src/MSBuildLocator/NativeMethods.cs
/// and https://github.com/dotnet/sdk/blob/2b7468dad75a2c8dc419cd30da4443ac6d699aa6/src/Resolvers/Microsoft.DotNet.NativeWrapper/NETCoreSdkResolverNativeWrapper.cs.
/// </remarks>
public class ValidateGlobalJsonSdkVersion : Task
{
    /// <summary>
    /// The working directory to search for global.json.
    /// Typically set to $(MSBuildProjectDirectory).
    /// </summary>
    [Required]
    public string WorkingDir { get; set; } = string.Empty;

    internal const string HostFxrName = "hostfxr";

    internal enum hostfxr_resolve_sdk2_flags_t
    {
        disallow_prerelease = 0x1,
    }

    internal enum hostfxr_resolve_sdk2_result_key_t
    {
        resolved_sdk_dir = 0,
        global_json_path = 1,
        requested_version = 2,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    internal delegate void hostfxr_resolve_sdk2_result_fn(
        hostfxr_resolve_sdk2_result_key_t key,
        string value);

    [DllImport(HostFxrName, CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int hostfxr_resolve_sdk2(
        string exe_dir,
        string working_dir,
        hostfxr_resolve_sdk2_flags_t flags,
        hostfxr_resolve_sdk2_result_fn result);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void hostfxr_error_writer_fn(IntPtr message);

    [DllImport(HostFxrName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr hostfxr_set_error_writer(IntPtr error_writer);

    public override bool Execute()
    {
        bool found = false;
        bool requestsVersion = false;

        // Prevent hostfxr from writing to stderr
        hostfxr_error_writer_fn swallowErrors = new(message => { });
        IntPtr errorWriter = Marshal.GetFunctionPointerForDelegate(swallowErrors);
        IntPtr? previousErrorWriter = null;

        try
        {
            previousErrorWriter = hostfxr_set_error_writer(errorWriter);

            // Create the result callback delegate
            hostfxr_resolve_sdk2_result_fn resultCallback = (key, value) =>
            {
                if (key == hostfxr_resolve_sdk2_result_key_t.global_json_path)
                {
                    found = true;
                }

                if (key == hostfxr_resolve_sdk2_result_key_t.requested_version)
                {
                    requestsVersion = true;
                }
            };

            hostfxr_resolve_sdk2(exe_dir: string.Empty, working_dir: WorkingDir, flags: 0, result: resultCallback);
            GC.KeepAlive(resultCallback);
        }
        finally
        {
            if (previousErrorWriter is not null)
            {
                hostfxr_set_error_writer(previousErrorWriter.Value);
            }
            GC.KeepAlive(swallowErrors);
        }

        bool globalJsonEnforcesSdkVersion = found && requestsVersion;
        if (!globalJsonEnforcesSdkVersion)
        {
            Log.LogWarning(
                subcategory: null,
                warningCode: "RPB0003",
                helpKeyword: null,
                helpLink: "https://github.com/dotnet/reproducible-builds/blob/main/docs/diagnostics/RPB0003.md",
                file: null,
                lineNumber: 0,
                columnNumber: 0,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: "global.json not found, or does not specify an SDK version. Add one to ensure a consistent build environment.",
                messageArgs: null);
        }

        return true;
    }
}
