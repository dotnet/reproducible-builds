﻿# DotNet.ReproducibleBuilds

[![.NET Foundation](https://img.shields.io/badge/.NET%20Foundation-blueviolet.svg)](https://www.dotnetfoundation.org/)
[![Build Status](https://dev.azure.com/dotnet/Projects/_apis/build/status%2FReproducibleBuilds%20-%20CI?branchName=main)](https://dev.azure.com/dotnet/Projects/_build/latest?definitionId=154&branchName=main)

This repo is a collection of best practices for build reproducibility with MSBuild.

It provides documentation and NuGet packages to simplify build configuration and isolate builds from developer or
workstation-specific settings.

## DotNet.ReproducibleBuilds nuget package

[![NuGet Version](https://img.shields.io/nuget/v/DotNet.ReproducibleBuilds?style=flat&label=DotNet.ReproducibleBuilds)](https://www.nuget.org/packages/DotNet.ReproducibleBuilds)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DotNet.ReproducibleBuilds?style=flat)](https://www.nuget.org/packages/DotNet.ReproducibleBuilds)

It's highly recommended that all projects enable these settings, either via
adding this package or manually as described here: https://devblogs.microsoft.com/dotnet/producing-packages-with-source-link/

This package sets the following properties:
- `PublishRepositoryUrl` = `true`
- `DebugType` = `embedded`. You can specify `portable` in your project if you prefer, but you'll need to upload that `.snupkg` file too.
- `ContinuousIntegrationBuild` = `true` on CI systems

More information on `PublishRepositoryUrl` and debugging with Source Link is [here](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

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

[![NuGet Version](https://img.shields.io/nuget/v/DotNet.ReproducibleBuilds.Isolated?style=flat&label=DotNet.ReproducibleBuilds.Isolated)](https://www.nuget.org/packages/DotNet.ReproducibleBuilds.Isolated)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DotNet.ReproducibleBuilds.Isolated?style=flat)](https://www.nuget.org/packages/DotNet.ReproducibleBuilds.Isolated)

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
