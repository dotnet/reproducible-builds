# AssemblySearchPaths

The `AssemblySearchPaths` MSBuild property controls how MSBuild target `ResolveFrameworkReferences` resolves the location of the selected `TargetFramework` assemblies. By default, old style projects will include the GAC and other outside-of-repo locations that can be impacted by other software that is installed on the developer machine.

- Recommendation: Always
- Impact: Low

## Usage

In Directory.Build.props

```xml
<PropertyGroup>
  <AssemblySearchPaths>
    {CandidateAssemblyFiles};
    {HintPathFromItem};
    {TargetFrameworkDirectory};
    {RawFileName};
  </AssemblySearchPaths>
</PropertyGroup>
```

## References 

- [MSBuild target framework and target platform](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-target-framework-and-target-platform?view=vs-2019)
