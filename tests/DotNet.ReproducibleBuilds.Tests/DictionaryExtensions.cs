namespace DotNet.ReproducibleBuilds.Tests;

internal static class DictionaryExtensions
{
    public static IDictionary<TKey, TValue> With<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        => new Dictionary<TKey, TValue>(dictionary) { [key] = value };
}
