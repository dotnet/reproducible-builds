# DotNet.ReproducibleBuilds

This repo generates a package that enables reproducible builds in a single step. It's highly recommended that all projects enable these settings, either via
adding this package or manually as described here: https://devblogs.microsoft.com/dotnet/producing-packages-with-source-link/

This package sets the following properties:
- `PublishRepositoryUrl` = `true`
- `EmbedUntrackedSources` = `true`
- `DebugType` = `embedded`
- `IncludePackageReferencesDuringMarkupCompilation` = `true` (enables a fix for WPF)
- `ContinuousIntegrationBuild` = `true` on CI systems

It also adds SourceLink dependencies for all repo types (the right one will be used automatically).

## Usage

Add the following to your `Directory.Build.props` file so all projects in your solution have the package added -- use the latest package version.

```xml
<ItemGroup>
  <PackageReference Include="DotNet.ReproducibleBuilds" Version="0.1.7" PrivateAssets="All"/>
</ItemGroup>
```

Prerelease packages are available on the following [NuGet feed](https://dev.azure.com/dotnet/Projects/_packaging?_a=feed&feed=ReproducibleBuilds):
`https://pkgs.dev.azure.com/dotnet/Projects/_packaging/ReproducibleBuilds/nuget/v3/index.json`

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for information on contributing to this project.

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) 
to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).

## License

This project is licensed with the [MIT license](LICENSE).

## .NET Foundation

New Repo is a [.NET Foundation project](https://dotnetfoundation.org/projects).

## Related Projects

You should take a look at these related projects:

- [.NET Core](https://github.com/dotnet/core)
- [ASP.NET](https://github.com/aspnet)
- [Mono](https://github.com/mono)