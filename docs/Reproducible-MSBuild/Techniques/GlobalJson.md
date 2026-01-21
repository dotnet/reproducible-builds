# GlobalJson

Without a global.json file specifying an SDK version, the .NET CLI uses the latest installed SDK. This can lead to inconsistent builds across different machines or over time as new SDK versions are installed.

The `DotNet.ReproducibleBuilds.Isolated` package validates that a global.json file exists and specifies an SDK version. If not, it emits warning [RPB0003](../../diagnostics/RPB0003.md).

- Recommendation: Always
- Impact: Low

## Usage

Create a global.json file in your repository root:

```bash
dotnet new globaljson
```

This creates a file like:

```json
{
  "sdk": {
    "version": "10.0.100"
  }
}
```

You can also specify roll-forward behavior:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestPatch"
  }
}
```

## Remarks

The global.json file should be committed to source control. This ensures all developers and CI systems use the same SDK version, producing consistent build outputs.

## References

- [global.json overview](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)
- [.NET SDK versioning](https://learn.microsoft.com/en-us/dotnet/core/versions/)
