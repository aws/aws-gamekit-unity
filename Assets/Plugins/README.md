# AWS GameKit Tests - Plugins
This folder contains binaries for the [Moq library](https://github.com/moq/moq4) and its dependencies.

## Instructions for Updating Moq
Follow these instructions when updating Moq to a newer version:
1. Download the latest Windows x86 Commandline from here: https://www.nuget.org/downloads
2. Open a PowerShell terminal in the directory where you downloaded `nuget.exe`
3. Run these commands (replace with the desired version):
    * `mkdir NewMoq`
    * `cd NewMoq`
    * `..\nuget.exe install Moq -Version 4.16.1`
4. Delete the existing DLL files in this `Plugins/` folder.
5. Copy the following files into this `Plugins/` folder:
    * NewMoq > Castle.Core.X.Y.Z > lib > netstandard1.5 > Castle.Core.dll
        * Note: Castle.Core currently doesn't provide a binary for netstandard2.0. The netstandard1.5 binary is compatible with the
          netstandard2.0 Moq.dll.
    * NewMoq > Moq.X.Y.Z > lib > netstandard2.0 > Moq.dll
    * NewMoq > System.Runtime.CompilerServices.Unsafe.X.Y.Z > lib > netstandard2.0 > System.Runtime.CompilerServices.Unsafe.dll
    * NewMoq > System.Threading.Tasks.Extensions.X.Y.Z > lib > netstandard2.0 > System.Threading.Tasks.Extensions.dll
    * (plus any other dependencies that may have been added in a newer version of Moq)
6. If needed, re-add `Moq.dll` as an `Assembly Reference` on each of the four assembly definition files
(ex: `Assets/AWS GameKit Tests/UnitTests/Editor/AWS.GameKit.Editor.UnitTests.asmdef`).

## .NET Standard 2.0
Note: The AWS GameKit Plugin uses .NET Standard 2.0.

If this is ever upgraded (ex: to .NET Standard 2.1), then the Moq binaries need to be updated accordingly
(ex: use `Moq.X.Y.Z > lib > netstandard2.1 > Moq.dll` instead of `netstandard2.0`, etc.).
