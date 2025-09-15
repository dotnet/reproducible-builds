using System.Diagnostics.CodeAnalysis;

namespace DotNet.ReproducibleBuilds.Tests.Shared;

internal static class BooleanExtensions
{
    public static string ToLowerInvariant(this bool value) => value.ToString().ToLowerInvariant();
    public static string? ToLowerInvariant([NotNullIfNotNull(nameof(value))] this bool? value) => value?.ToString().ToLowerInvariant();
}
