namespace DotNet.ReproducibleBuilds.Tests.Shared;

internal sealed class EnvironmentVariableSuppressor : IDisposable
{
    private readonly string? _value;
    private readonly string _name;

    public EnvironmentVariableSuppressor(string name)
    {
        _name = name;
        _value = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, null);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(_name, _value);
    }
}
