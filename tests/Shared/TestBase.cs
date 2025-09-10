namespace DotNet.ReproducibleBuilds.Tests.Shared;

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

    protected string GetCurrentTargetFrameworkMoniker()
    {
#if NETFRAMEWORK
        return "net472";
#elif NET8_0
        return "net8.0";
#elif NET9_0
        return "net9.0";
#elif NET10_0
        return "net10.0";
#else
        throw new NotSupportedException("Unsupported target framework.");
#endif
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
