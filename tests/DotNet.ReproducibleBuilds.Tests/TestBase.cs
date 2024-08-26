namespace DotNet.ReproducibleBuilds.Tests;

public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        TestRootPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())).FullName;
    }

    public string TestRootPath { get; }

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (Directory.Exists(TestRootPath))
        {
            try
            {
                Directory.Delete(TestRootPath, recursive: true);
            }
            catch (Exception)
            {
                // Ignored
            }
        }
    }

    protected string GetTempFileName(string? extension = null)
    {
        return Path.Combine(TestRootPath, $"{Path.GetRandomFileName()}{extension ?? string.Empty}");
    }

    protected string GetTempProjectPath(string? extension = null)
    {
        DirectoryInfo tempDirectoryInfo = Directory.CreateDirectory(Path.Combine(TestRootPath, Path.GetRandomFileName()));

        return Path.Combine(tempDirectoryInfo.FullName, $"{Path.GetRandomFileName()}{extension ?? string.Empty}");
    }
}
