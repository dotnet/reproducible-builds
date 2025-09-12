# DisableImplicitLibraryPacksFolder

Similar to [`DisableImplicitNuGetFallbackFolder`](./DisableImplicitNuGetFallbackFolder.md), Microsoft.NET.NuGetOfflineCache.targets (imported by Microsoft.NET.Sdk.BeforeCommon.targets) adds an extra search path for nuget packages that is not controlled by the user nuget.config. It will add the `sdk/<version>/FSharp` from your dotnet installation to the set of nuget search locations. At the time of writing, this location is used to distribute the FSharp.Core NuGet package, which is generally referenced by all F# projects (and, transitively, all projects that reference projects and NuGet packages created with F#). This could potentially poison your nuget cache between repos, even if you've configured per-repo nuget caches.


## Usage

In Directory.Build.props

```xml
<PropertyGroup>
  <DisableImplicitLibraryPacksFolder>true</DisableImplicitLibraryPacksFolder>
</PropertyGroup>
```

## References
- [Microsoft.NET.NuGetOfflineCache.targets](https://github.com/dotnet/sdk/blob/main/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.NuGetOfflineCache.targets)
- Official documentation: none?
