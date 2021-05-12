# Reproducible MSBuild

If you're landing here, you're probable interested in making your builds more repeatable or improve build reliable in some way. This wiki records a number of techniques and tricks for making MSBuild based builds repeatable through isolation from other installed software on your build machines.

This follows a few princples:

## The build should isolate itself from installed software and settings of the host machine

Generally speaking, your repo should describe which build tools it needs, and either install or only use the specific tools and versions that it's configured for. 

> E.g. my build requires Powershell for some custom build steps. Instead of requiring the user to add a specific version of Powershell.exe to the path, I'll instead install it via dotnet tool restore. This also ensures that my build gets the version of Powershell it expects -- Powershell 5.1 to 7.0 is neither forward nor backwards compatible with each other.

## The build should not leave any lasting change on the host machine

Additionally, your build should not leave any cruft behind that could impact the user. Not all repos are following Repeatable Build philosophies -- yet we would still likely to ensure that building our repos don't break the user's other repos on the same machine.

Anything "installed" during your build should be removable by `git clean` on the repo root.

> E.g. my build requires a specific Powershell module for some custom build steps. Rather than installing it into the global or user cache, I configure my build to install it into the `packages/` folder under my repo, and configure `packages/` in `.gitignore`. 

## The repo should be self describing

If your repo has prerequisites, it's often polite to create an init.cmd that locally installs these prerequisites. If not, your README.md should indicate what prerequisites the user must install. 

Additionally, strongly prefer prerequisites that can be installed side-by-side. For instance, the dotnetsdk can be "installed" by adding it to the path and disabling multi-level-lookup. 

> E.g. my README.md instructs users that they need only install dotnetsdk according to the version in global.json, and the repo takes care of everything else. The user can either install it normally, or install it 'standalone'. The build lab always installs it 'standalone' to validate that we can always build with just one version installed.

## Tool versions matter

Every tool used in the build should be pre-configured to support locking to an older version, in case of regression in the build tooling. Regressions include user "bugs" that the build tooling used to accept in a previous reslease.

This guide generally prefers using dotnetsdk over Visual Studio as dotnetsdk's install script supports a -Version argument to easily lock a repo to an earlier sdk version until a regression can be addressed.

> E.g. my global.json is currently configured for 3.1.407 and latestPatch. However, if a new version regresses something, I know I can easily edit global.json to lock it back to 3.1.407 precisely. This unblocks my main branch builds, and gives me time to file a bug against the dotnetsdk for the regression and seek a workaround.





