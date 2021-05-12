# TargetFrameworkRootPath

The "GetReferenceAssemblyPaths" target in "Microsoft.Common.CurrentVersion.targets" resolves from Program Files and Windows Registry where it will search for .net framework reference assemblies (e.g. `<Reference>`).  Instead, recommend utilizing the Microsoft.NETFramework.ReferenceAssemblies nuget instead. Version 1.0.0 of this nuget will provide framework references for all desktop framework versions. 

The `TargetFrameworkRootPath` property lets the user override this process and choose a different location to provide the target framework. By overriding this, we can prevent accidentally picking up target framework installations from registry or program files locations.

- Recommendation: Always
- Impact: Low

## Usage

In Directory.Build.props

```xml
<!-- Disable the normal search path mechanism -->
<PropertyGroup>
  <TargetFrameworkRootPath Condition="'$(BuildingInsideVisualStudio)'!='true'">[UNDEFINED]</TargetFrameworkRootPath>
</PropertyGroup>
<Target Name="CheckTargetFrameworkRootPath" BeforeTargets="GetReferenceAssemblyPaths" Condition="'$(BuildingInsideVisualStudio)'!='true'">
  <Warning
    Condition="'$(TargetFrameworkRootPath)' == '[UNDEFINED]' and '$(TargetFrameworkIdentifier)' == '.NETFramework'"
    Text="Error, TargetFrameworkRootPath not initialized. If you're building for net462 or any other version of desktop NETFramework, please reference the 'Microsoft.NETFramework.ReferenceAssemblies' nuget package and run restore on the project to fix up your framework reference paths." />
</Target>
```

Alternatively, you may opt to force importing of the Microsoft.NETFramework.ReferenceAssemblies package. However, this should only be performed in repos that are no longer using packages.config

```xml
<!-- Disable the normal search path mechanism -->
<PropertyGroup>
  <TargetFrameworkRootPath Condition="'$(BuildingInsideVisualStudio)'!='true'">[UNDEFINED]</TargetFrameworkRootPath>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" Version="1.0.0" VersionOverride="1.0.0" />
</Target>
```

## Remarks

Not only does GetReferenceAssemblyPaths rely on installed software, there are different versions of these reference assemblies in the different versions of MSBuild / Visual Studio available. This can result in non-repeatable assembly reference errors (e.g. System.Net.Http appears as version 4.0.0.0 in one version of the net462 reference assemblies, but version 4.2.0.0 in another). This is thought to have behavioral differences.

Using the nuget package will produce consistent results regardless of installed reference assemblies, as it overrides the path resolved by GetReferenceAssemblyPaths.

## References
- [GetReferenceAssemblyPaths task](https://docs.microsoft.com/en-us/visualstudio/msbuild/getreferenceassemblypaths-task?view=vs-2019)
- Github issue, closed: [TargetFrameworkRootPath not respected](https://github.com/dotnet/msbuild/issues/598)