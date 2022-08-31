# AWS GameKit Cloud Resources - Instance Files

## What is the InstanceFiles folder?
This folder will be empty until you deploy your first feature.

When a feature is deployed for the first time in an environment, a new folder will automatically be created in the
InstanceFiles folder. The folder, titled `<gameName>/<environment>/<regionCode>`, will contain a copy of all needed
resources from the BaseTemplates folder. 

On subsequent deployments to the same environment, your instance files in `<gameName>/<environment>/<regionCode>` will
not be overwritten by BaseTemplate files. If you wish to change any functionality of a feature, do so in the InstanceFiles
folder corresponding to the environment you want to change. Changes to any BaseTemplates files will only affect deployments
for the first time to an environment+regionCode combination.

## Content Description
The following sections describe the content that will be created under `InstanceFiles/<gameName>/<environment>/<regionCode>`
when you deploy the first feature to an  environment+regionCode combination:

### /cloudformation
Contains AWS CloudFormation Templates for every feature. These templates model "infrastructure-as-code" for the cloud resources
that will get deployed for each feature.

To learn more about customizing AWS CloudFormation Templates, please visit the following page:

https://aws.amazon.com/cloudformation/resources/templates/

### /functions
Contains all the AWS Lambda functions associated with each feature.

To learn more about AWS Lambda functions, please visit the following page:

https://aws.amazon.com/lambda/

### /layers
Contains AWS Lambda layers, which are common code that are shared across multiple AWS Lambda functions.

To learn more about AWS Lambda layers, please visit the following page:

https://docs.aws.amazon.com/lambda/latest/dg/invocation-layers.html

### awsGameKitClientConfig.yml
Contains parameters associated with your deployed resources. The awsGameKitClientConfig.yml associated with your
currently selected environment will be packaged when building your game.

To learn more, check out the README in `Packages/com.amazonaws.gamekit/Resources/`