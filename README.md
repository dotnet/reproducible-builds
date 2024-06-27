# DotNet.ReproducibleBuilds

[![.NET Foundation](https://img.shields.io/badge/.NET%20Foundation-blueviolet.svg)](https://www.dotnetfoundation.org/)
[![Build Status](https://dev.azure.com/dotnet/Projects/_apis/build/status%2FReproducibleBuilds%20-%20CI?branchName=main)](https://dev.azure.com/dotnet/Projects/_build/latest?definitionId=154&branchName=main)
![NuGet Version](https://img.shields.io/nuget/v/DotNet.ReproducibleBuilds?style=flat&label=DotNet.ReproducibleBuilds)
![NuGet Version](https://img.shields.io/nuget/v/DotNet.ReproducibleBuilds.Isolated?style=flat&label=DotNet.ReproducibleBuilds.Isolated)

This repo generates a package that enables reproducible builds in a single step, and documents MSBuild settings useful for enabling reproducibility through isolation.

This repo documents various MSBuild settings for reproducibilty, and providing two nuget packages for enabling some of these setting.

The packages are:

- DotNet.ReproducibleBuilds
- DotNet.ReproducibleBuilds.Isolated


## DotNet.ReproducibleBuilds nuget package

It's highly recommended that all projects enable these settings, either via
adding this package or manually as described here: https://devblogs.microsoft.com/dotnet/producing-packages-with-source-link/

This package sets the following properties:
- `PublishRepositoryUrl` = `true`
- `EmbedUntrackedSources` = `true`
- `DebugType` = `embedded`. You can specify `portable` in your project if you prefer, but you'll need to upload that `.snupkg` file too.
- `IncludePackageReferencesDuringMarkupCompilation` = `true` (enables a fix for WPF)
- `ContinuousIntegrationBuild` = `true` on CI systems

It also adds SourceLink dependencies for all repo types (the right one will be used automatically).

For more information on debugging with Source Link is [here](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

### Usage

Add the following to your `Directory.Build.props` file so all projects in your solution have the package added -- use the latest package version.

```xml
<ItemGroup>
  <PackageReference Include="DotNet.ReproducibleBuilds" Version="1.2.4" PrivateAssets="All"/>
</ItemGroup>
```

MSBuild 16.10 is required to generate binaries that can be fully reproduced. You'll need Visual Studio 2019 16.10 and/or .NET 5.0.300 SDK. You'll get a warning 
if you're using a lower version.

Prerelease packages are available on the following [NuGet feed](https://dev.azure.com/dotnet/Projects/_packaging?_a=feed&feed=ReproducibleBuilds):
`https://pkgs.dev.azure.com/dotnet/Projects/_packaging/ReproducibleBuilds/nuget/v3/index.json`

## DotNet.ReproducibleBuilds.Isolated Documentation and nuget package

It's highly recommended that all projects enable these settings, either via 
adding this package or manually, as described in [Documentation/Reproducible-MSBuild](Documentation/Reproducible-MSBuild/README.md).

This package configures a variety of properties and item groups to prevent your build from unintentionally 
depending on other installed software that's not described by your repo. All build dependencies should come
from either the MSBuild SDK you've chosen, or from nuget packages restored from your package feed. 

If you check out the same commit with the same SDK version and same nuget feed, you should get the same build result.

### Usage

Add the following to the top of your projects or to `Directory.Build.props`:

```xml
<Sdk Name="DotNet.ReproducibleBuilds.Isolated" Version="1.2.4" />
```

Tested on MSBuild 16.7 (Latest LTS at time of writing).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for information on contributing to this project.

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) 
to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).

## License

This project is licensed with the [MIT license](LICENSE).

## .NET Foundation

DotNet.ReproducibleBuilds is a [.NET Foundation project](https://dotnetfoundation.org/projects).

## Related Projects

You should take a look at these related projects:

- [.NET Core](https://github.com/dotnet/core)
- [ASP.NET](https://github.com/aspnet)
- [Mono](https://github.com/mono)
