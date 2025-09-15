namespace DotNet.ReproducibleBuilds.Tests.Shared;

internal static class CollectionExtensions
{
    public static IDisposable ToDisposable(this IEnumerable<IDisposable> disposables) => new DisposableCollection(disposables);
}
