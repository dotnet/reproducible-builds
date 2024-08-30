namespace DotNet.ReproducibleBuilds.Tests;

public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        TestRootPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
    }

    public DirectoryInfo TestRootPath { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        TestRootPath.Refresh();
        if (TestRootPath.Exists)
        {
            try
            {
                TestRootPath.Delete(recursive: true);
            }
            catch (Exception)
            {
                // Ignored
            }
        }
    }

    protected FileInfo GetRandomFile(string? extension = null)
    {
        return new(TestRootPath.Combine($"{Path.GetRandomFileName()}{extension ?? string.Empty}"));
    }
}
