# AWS GameKit Cloud Resources
This folder contains all the necessary configurations and code that AWS GameKit uses to deploy the cloud-based backend for each feature. 
It includes AWS Cloudformation templates and AWS Lambda functions and layers. This folder is not included in your project build. 
Access these files from the Editor only.
The subfolders InstanceFiles and .BaseTemplates each contain separate versions of the deployment configuration files, as described below.

This README will describe the key difference between the two folders (`.BaseTemplates` and `InstanceFiles`) For more
information on the `InstanceFiles` folder, check out the `README.md` in `Packages/com.amazonaws.gamekit/Editor/CloudResources/InstanceFiles`.

## Instance Files
When deploying a feature for the first time in an environment or region, a new directory inside of the `InstanceFiles`
folder will be created. The folder will be titled `<gameName>/<environment>/<regionCode>`. If a feature is being deployed
for the first time it will copy over all needed resources from the `.BaseTemplates` for that specific feature into the
`<gameName>/<environment>/<regionCode>` folder.

If you are making changes to cloud code inside your instance files, such as changing how one of the Lambda functions
works, you can change the necessary files inside its corresponding folder `InstanceFiles/<gameName>/<environment>/<regionCode>`.
After all desired files are changed the feature must be redeployed in order for the changes to take place.

IMPORTANT: If you need a change to be made across many environments and regions you will need to modify the required 
files in each of the `<environment>/<regionCode>` folders. 

## Base Templates
If you do not see a Base Templates folder that is because it is a hidden folder under the name `.BaseTemplates`. The Base
Templates folder contains the starting point for each cloud feature. The reason this folder was made hidden is because it
should not be edited unless you are sure you want a change to affect all new deployments. 

When deploying a feature for the first time all the required code for that feature, inside of `.BaseTemplates` folder will
be copied over to the newly created `InstanceFiles/<gameName>/<environment>/<regionCode>` folder.

IMPORTANT: If any changes are made to the `.BaseTemplate` folder the changes will only effect newly deployed features.
Since the Base Templates are only copied over when deploying for the first time, they will not affect updating a feature.

