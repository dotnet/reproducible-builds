# Changelog

## [1.2.39]

### Added

- [Add codes and help links to existing warnings](https://github.com/dotnet/reproducible-builds/pull/68) - thanks @MattKotsenas!

## [1.2.25]

### Added

- [Align RepositoryBranch logic with .NET 9](https://github.com/dotnet/reproducible-builds/pull/50) - thanks @MattKotsenas!

### Removed

- [Bump required SDK to 8+ and remove fixed issues](https://github.com/dotnet/reproducible-builds/pull/52) - thanks @MattKotsenas!

## [1.2.4]

### Added

- [Set the `DisableImplicitLibraryPacks` property to `true` to prevent using packages from the .NET SDK's built-in package sources.](https://github.com/dotnet/reproducible-builds/pull/21) - thanks @cmeeren!
- [Populate the `RepositoryBranch` property if other Repository Metadata has been requested to be made public.](https://github.com/dotnet/reproducible-builds/pull/27)  - thanks @kzu!

### Removed

- [No longer include the .NET Framework reference assembly packages, because the .NET SDK does this now.](https://github.com/dotnet/reproducible-builds/pull/33) - thanks @MattKotsenas!

## [1.2.0]

### Added

- [Support for transitive package references](https://github.com/dotnet/reproducible-builds/pull/16) - thanks @meziantou!
- [Add RepositoryBranch package metadata when available in the build environment](https://github.com/dotnet/reproducible-builds/pull/27) - thanks @kzu! 

### Removed

- [Remove the automatically provided .NET Framework reference assembly PackageReference, since the SDK provides this already](https://github.com/dotnet/reproducible-builds/pull/33) - thanks @MattKotsenas!
