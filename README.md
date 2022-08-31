# AWS GameKit Unity Package
This repository contains modifiable source code for the AWS GameKit package for Unity in the form of an [Embedded Package](https://docs.unity3d.com/Manual/CustomPackages.html#EmbedMe). If you don’t need to modify the AWS GameKit package, you can download the latest tarball `.tgz` file from the [releases page](https://github.com/aws/aws-gamekit-unity/releases) and add it to a Unity project using the Unity Package Manager.
To modify this package we recommend cloning this repo and opening it as a blank Unity project. After making your changes, follow the Packaging steps below to generate a custom plugin package, which you can then add to your Unity projects.

See: [Packages/com.amazonaws.gamekit/README.md](Packages/com.amazonaws.gamekit/README.md)

## Dev Setup

### Clone this repository
```
git clone https://github.com/aws/aws-gamekit-unity
```

## Add GameKit C++ SDK binaries
This package needs the GameKit C++ SDK binaries in order to work. There are two ways to get the binaries:

Use pre-built binaries:
1. Download the latest AWS GameKit Unity Package from the [releases page](https://github.com/aws/aws-gamekit-unity/releases).
1. Expand the `com.amazonaws.gamekit-<version>.tgz` file.
1. Copy the folder `<expanded contents>/package/Plugins/]` into `<root of this repo>/Package/com.amazonaws.gamekit/Plugins` and overwrite any existing files with the same name.

Build the binaries:
1. Clone and setup the [AWS GameKit C++ SDK](https://github.com/aws/aws-gamekit) using the tag specified in the [.gkcpp_version](.gkcpp_version) file in this repository.
1. Follow these steps each time you modify the C++ SDK:
    1. Close Unity. Otherwise the `.meta` files for the binaries will be deleted by Unity in the next step.
    1. Compile the [AWS GameKit C++ SDK](https://github.com/aws/aws-gamekit#aws-gamekit-c-sdk).
    1. Run `refresh_plugin.py` as described in the `Update Plugin with new binaries and headers` section of the
    [AWS GameKit C++ SDK README](https://github.com/aws/aws-gamekit#update-plugin-with-new-binaries-and-headers).

### Add this Unity project to Unity Hub
1. Open Unity Hub.
1. Open the "Projects" tab along the left side.
1. Click "Add", navigate to the root of the repository, then click "Select Folder".

### Generate the C# Project
1. Open Unity Hub.
1. Open the `aws-gamekit-unity` project.
1. In the Project window, navigate to `Packages/AWS GameKit/Runtime/Scripts`.
1. Double click on any C# file.  
*This will open the C# project in your IDE. Unity will automatically create the project if none exists. If you haven't set up an IDE or code editor, follow the guide in the [Unity documentation](https://docs.unity3d.com/Manual/ScriptingToolsIDEs.html).*

If you ever need to regenerate the solution:
1. Open Unity.
1. Go to `Edit > Preferences` and click `Regenerate project files`

## Unit Tests
See [Assets/AWS_GameKit_Tests/README.md](Assets/AWS_GameKit_Tests/README.md) for instructions on running the unit tests.

## Debugging
### Unity
Follow these instructions to debug the C# code: https://docs.unity3d.com/Manual/ManagedCodeDebugging.html

### Debugging AWS GameKit C++ SDK on Windows
Follow these instructions to debug the [AWS GameKit C++ SDK](https://github.com/aws/aws-gamekit) on Windows and step through from
C# into C++ and back. You'll have two instances of Visual Studio open: one for C# and one for C++.

One-time setup:
1. Follow the steps in section `Optional - Building the GameKit C++ SDK` above, ensure Debug binaries are being created instead of release.
1. Open the AWS GameKit C++ SDK solution file in Visual Studio.
1. Add the C++ debugging symbols to Visual Studio:
    * Debug > Options > Debugging > Symbols > + (top right) >
        * Add the full path to `/Packages/com.amazonaws.gamekit/Plugins/Windows/x64`

Each time:
1. Open the AWS GameKit C++ SDK solution file in Visual Studio.
1. Attach the Visual Studio debugger to the Unity project. In Visual Studio:
    * Debug > Attach to Process >
        * Process: `Unity.exe`
        * Title (Example): `aws-gamekit-unity - Untitled Scene - PC, Mac & Linux`
1. Open AWS GameKit Unity in a separate Visual Studio window.
1. Attach the debugger to Unity (see previous section titled `Debugging > Unity`).
1. Set breakpoints.
1. Trigger breakpoints via `Play Mode` or by interacting with AWS GameKit through the Unity editor.

## Packaging
Run the following commands in order to create the `com.amazonaws.gamekit-<version>.tgz` file:
```
cd path/to/aws-gamekit-unity
python export_unitypackage.py UNITY_APPLICATION_FULL_PATH
```

Where `UNITY_APPLICATION_FULL_PATH` is like:
* Windows: `"C:\Program Files\Unity\Hub\Editor\2021.3.4f1\Editor\Unity.exe"`
* Mac: `/Applications/Unity/Hub/Editor/2021.3.4f1/Unity.app/Contents/MacOS/Unity`

## Installing Package Manager Package
See [Packages/com.amazonaws.gamekit/README.md](Packages/com.amazonaws.gamekit/README.md) for instructions on installing the
Package Manager package after it has been created.
