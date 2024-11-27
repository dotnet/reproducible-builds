# NUGET_XMLDOC_MODE

The environment variable NUGET_XMLDOC_MODE can affect the behavior of nuget restore, both via nuget.exe and via dotnet restore. Make sure this environment variable is cleared, or set to 'none', in order to get default behavior.

The intent of this setting is to omit unpacking xml doc files packaged with some nuget packages. However, it's actual behavior is to suppress all .xml files distributed inside a nuget package, including non-documentation files which may be critical to the nuget package's operation.

- Recommendation: Always
- Impact: Low

## Usage

In your build system, make sure to clear this environment variable. 

If using Azure DevOps, you can clear the variable with the following additions to your YAML definition:

```yaml
variables:
  NUGET_XMLDOC_MODE: ''
```

Unfortunately there is no way known at this time to override this setting via MSBuild properties. Each developer will need to check if any installed program has changed this setting.

## Remarks

The dotnet sdk docker image sets NUGET_XMLDOC_MODE=skip ([issue 2790](https://github.com/dotnet/dotnet-docker/issues/2790)), which causes builds run under this docker image to produce results different from a typical developer workstation. Thousands of developers are using this build image for their lab builds in the Azure organization.
