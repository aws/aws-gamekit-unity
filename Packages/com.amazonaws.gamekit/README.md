# AWS GameKit Unity Package Manager Package

## About AWS GameKit
Use the AWS GameKit package for Unity to configure and deploy AWS resources to support each cloud-based game feature. Custom API libraries
and an API testing tab make it easy to incorporate feature functionality into front-end game code to communicate with backend services.

To learn more about AWS GameKit, check out the [AWS GameKit Documentation](https://docs.aws.amazon.com/gamekit/).

### Cloud Features:
#### [Identity](https://docs.aws.amazon.com/gamekit/latest/DevGuide/identity-auth.html):

The AWS GameKit identity and authentication feature provides tools for creating and storing unique player identifiers.
With AWS GameKit, Player IDs are used to manage player access, authenticate communications between game clients and
backend services, and other scenarios that require identity verification and authentication.

#### [Achievements](https://docs.aws.amazon.com/gamekit/latest/DevGuide/achievements.html):
The AWS GameKit achievements feature provides tools to manage an achievements system for your game. Also called
badges, awards, or challenges, achievements are a proven way to boost player engagement. By offering rewards for
accomplishing specific tasks, you can give players both short- and long-term goals to aim for and a sense of
accomplishment. Achievements can help define what objects, actions, or other things have value in your game. They
can be used to guide players through gameplay to help ensure an optimal player experience.

#### [User gameplay data](https://docs.aws.amazon.com/gamekit/latest/DevGuide/gameplay-data.html):
This game feature is designed to handle data synchronization on the fly during gameplay. User gameplay data might include
information such as player scores, inventory, or other player-associated data that is needed in the game. It is not
designed for other player data, such as for player profiles or account information.

#### [Game state cloud saving](https://docs.aws.amazon.com/gamekit/latest/DevGuide/game-save.html):
The AWS GameKit game state cloud saving feature enables players to save their game state, store it in the cloud, and
keep it synchronized with their local game clients. Game saving is a highly valued game feature for players, particularly
with games with very long storylines. Cloud saving and synchronization offers additional benefits to players, including:

- For games that support more than one game platform, players can get true crossplay portability on multiple devices.
- Players can recover game progress with minimal loss in the event of local device failures.
- Players still have access to past played game saves, even for games that have been uninstalled.

Use this game feature to implement an autosave system as well for explicit saves that allow players to choose their
save points.

## Installing AWS GameKit
To install this package, follow the instructions in the
[Unity Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

To learn more about Unity Packages, the official documentation can be found [here](https://docs.unity3d.com/Manual/Packages.html).

## AWS GameKit Example Games
The [AWS GameKit Unity Examples](https://github.com/aws/aws-gamekit-unity-examples) repository can be downloaded to
get a Unity project with the AWS GameKit package preinstalled. The example Unity project also includes fully functioning
games for each GameKit feature with code that can be used as a starting point for your own game.

## Using AWS GameKit
For information on using AWS GameKit check out the AWS GameKit Developer guide [here](https://docs.aws.amazon.com/gamekit/latest/UnityDevGuide/).

### Unity Versions
The version of the Unity Editor that has been tested and is recommended is 2021 LTS. You are free to use prior versions
(or upcoming beta versions) as most should work just fine but have not been thoroughly tested.
