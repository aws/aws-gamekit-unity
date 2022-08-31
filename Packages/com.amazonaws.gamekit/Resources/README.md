# AWS GameKit Resources
This folder will contain a file named `awsGameKitClientConfig.yaml` once you submit your AWS Credentials the first time.

This file needs to be included in your built game in order for the GameKit feature APIs to work.
It automatically gets included with your built game because this folder is named "Resources", which is a special folder 
name defined by Unity. See: https://docs.unity3d.com/Manual/SpecialFolders.html

The config file's settings are specific to the currently selected environment (i.e. "dev", "qa", "prd", etc.).
You can change the environment in the "Environment & Credentials" page of the AWS GameKit Settings Window.

IMPORTANT: Before building your game's executable, you must switch to the appropriate GameKit environment in the 
"Environment & Credentials" page of the AWS GameKit Settings Window. For example, switch to "prd" before building your 
production game.

The config file is automatically updated each time a GameKit feature is created or redeployed, or when the environment 
changes (i.e. "dev", "qa", "prd", etc.).
