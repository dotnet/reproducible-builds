# DotNet.ReproducibleBuilds

This repo generates a package that enables reproducible builds in a single step. It's highly recommended that all projects enable these settings, either via
adding this package or manually as described here: https://devblogs.microsoft.com/dotnet/producing-packages-with-source-link/

This package sets the following properties:
- `PublishRepositoryUrl` = `true`
- `EmbedUntrackedSources` = `true`
- `DebugType` = `embedded`. You can specify `portable` in your project if you prefer, but you'll need to upload that `.snupkg` file too.
- `IncludePackageReferencesDuringMarkupCompilation` = `true` (enables a fix for WPF)
- `ContinuousIntegrationBuild` = `true` on CI systems

It also adds SourceLink dependencies for all repo types (the right one will be used automatically).

For more information on debugging with Source Link is [here](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## Usage

Add the following to your `Directory.Build.props` file so all projects in your solution have the package added -- use the latest package version.

```xml
<ItemGroup>
  <PackageReference Include="DotNet.ReproducibleBuilds" Version="0.1.26" PrivateAssets="All"/>
</ItemGroup>
```

MSBuild 16.10 is required to generate binaries that can be fully reproduced. You'll need Visual Studio 2019 16.10 and/or .NET 5.0.300 SDK. You'll get a warning 
if you're using a lower version.

Prerelease packages are available on the following [NuGet feed](https://dev.azure.com/dotnet/Projects/_packaging?_a=feed&feed=ReproducibleBuilds):
`https://pkgs.dev.azure.com/dotnet/Projects/_packaging/ReproducibleBuilds/nuget/v3/index.json`

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
