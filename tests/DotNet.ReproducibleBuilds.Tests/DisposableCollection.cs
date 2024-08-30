namespace DotNet.ReproducibleBuilds.Tests;

internal sealed class DisposableCollection : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    public DisposableCollection(IEnumerable<IDisposable> disposables) => _disposables.AddRange(disposables);

    public void Add(IDisposable disposable) => _disposables.Add(disposable);

    public void Dispose()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
