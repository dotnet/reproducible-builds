# DisableImplicitNuGetFallbackFolder

By default, Microsoft.NET.NuGetOfflineCache.targets (imported by Microsoft.NET.Sdk.BeforeCommon.targets) will add an extra search path for nuget packages that is not controlled by the user nuget.config. It will add the `sdk/NuGetFallbackFolder` from your dotnet installation to the set of nuget search locations. This could potentially poison your nuget cache between repos, even if you've configured per-repo nuget caches.


## Usage

In Directory.Build.props

```xml
<PropertyGroup>
  <DisableImplicitNuGetFallbackFolder>true</DisableImplicitNuGetFallbackFolder>
</PropertyGroup>
```

## References
- [Microsoft.NET.NuGetOfflineCache.targets](https://github.com/dotnet/sdk/blob/main/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.NuGetOfflineCache.targets)
- Official documentation: none?
