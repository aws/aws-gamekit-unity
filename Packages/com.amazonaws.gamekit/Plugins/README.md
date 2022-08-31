# AWS GameKit Plugins
This folder contains the libraries for all platforms that are supported by AWS GameKit. 

IMPORTANT: The AWS GameKit Plugin only includes Release libraries due to Unity only allowing one set of libraries at a 
time. This means you will not be able to step into or perform deep debugging with the default GameKit libraries. If you
wish to build either Debug or Release libraries the instructions are available on the GameKit [Github repository](https://github.com/aws/aws-gamekit#aws-gamekit-c-sdk).

Unity will only include the libraries required for the selected build platform when building a standalone game. All libraries
that are not needed for the selected platform will be ignored and not affect the size of your standalone game. 
See: https://docs.unity3d.com/Manual/PluginInspector.html. 

If you or your team are only building for a specific platform you can remove unnecessary libraries in order to cut down on
the size of the Unity Project itself. However, even if you are developing for mobile only, the Editor code requires a copy
of the Mac Libraries and/or Windows Libraries. 
