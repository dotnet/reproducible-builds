﻿namespace DotNet.ReproducibleBuilds.Tests;

internal static class FileSystemInfoExtensions
{
    public static string Combine(this FileSystemInfo info, params string[] paths)
    {
        return Path.Combine([info.FullName, ..paths]);
    }
}
