# AWS GameKit Tests
This folder contains the Unit Tests for the AWS GameKit Unity Package.

These tests do not get bundled into the Unity Package (AwsGameKit.unitypackage).

The underlying AWS GameKit C++ binaries have their own tests defined in the
[aws/aws-gamekit repository](https://github.com/aws/aws-gamekit).

## Running the Tests
All tests are run inside the Unity Editor with the
[Unity Test Runner tool](https://docs.unity3d.com/2017.4/Documentation/Manual/testing-editortestsrunner.html).

The test runner is accessed through `Window > General > Test Runner`.

### Unit Tests
To run the unit tests:
1. Open the Unity Editor to this AWS GameKit project.
1. Open the Test Runner window through `Window > General > Test Runner`.
1. Select `PlayMode` or `EditMode`.
1. Select `AWS.GameKit.Editor|Runtime.UnitTests.dll`.
1. Click `Run Selected`.

## Plugins/Dependencies
The unit tests depend on the [Moq library](https://github.com/moq/moq4). The Moq binaries are included in
the [Assets/Plugins](../../Assets/Plugins/) folder.

See [Plugins/README.md](../Plugins/README.md) for more details.
