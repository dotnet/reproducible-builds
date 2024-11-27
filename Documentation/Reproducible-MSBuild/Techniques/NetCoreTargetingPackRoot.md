# NetCoreTargetingPackRoot

Similar to [`TargetFrameworkRootPath`](./TargetFrameworkRootPath.md), the MSBuild property `NetCoreTargetingPackRoot`
controls where dotnet sdk builds "sniff out" installed reference assemblies for dotnet runtimes. The target 
`ResolveTargetingPackAssets` will try seeing if there are any such reference assemblies installed into the 
`dotnet/packs/` folder. If not found, then MSBuild's built in targets will attempt to resolve the appropriate 
Microsoft.NETCore.App.Ref nuget package instead.

This can be problematic for reproducibility, as upgrading your .NET SDK can result in a different set of packs available, and possible build failures if that nuget package isn't available on your nuget feed.

- Recommendation: Always
- Impact: Low


## Usage

In Directory.Build.props

```xml
<PropertyGroup>
  <NetCoreTargetingPackRoot>[UNDEFINED]</NetCoreTargetingPackRoot>
</PropertyGroup>
```

## References

None. It appears this property, the ResolveTargetingPackAssets target, and ResolveTargetingPackAssets task are not properly documented at this time.
