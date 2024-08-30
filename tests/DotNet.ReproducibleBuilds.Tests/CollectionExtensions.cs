namespace DotNet.ReproducibleBuilds.Tests;

internal static class CollectionExtensions
{
    public static IDisposable ToDisposable(this IEnumerable<IDisposable> disposables) => new DisposableCollection(disposables);
}
