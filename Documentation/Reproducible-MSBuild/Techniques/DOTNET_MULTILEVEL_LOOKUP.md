# DOTNET_MULTILEVEL_LOOKUP

This environment variables controls whether dotnet.exe performs an extended search for additional
runtimes and sdks. By default, dotnet.exe in "net5.0" and earlier will search program files and registry 
for additional sdks and runtimes. This can result in selecting a different sdk that the user might have
expected.

- Recommendation: Sometimes
- Impact: Low

## Usage

In Azure Devops

```yaml
variables:
  DOTNET_MULTILEVEL_LOOKUP: 0
```

In Windows command line environments

```batch
set DOTNET_MULTILEVEL_LOOKUP=0
```

In Linux command line environments

```sh
export DOTNET_MULTILEVEL_LOOKUP=0
```


## References

- [Dotnet command / Environment Variables](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet#environment-variables)
- [Multi-level SharedFX Lookup](https://github.com/dotnet/core-setup/blob/master/Documentation/design-docs/multilevel-sharedfx-lookup.md)
