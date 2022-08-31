// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// GameKit
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Core
{
    /// <summary>
    /// Provides read/write access for persisting the user's AWS credentials to the standardized <c>~/.aws/credentials</c> file.<br/><br/>
    ///
    /// To learn more about this file, please see <see cref="https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-files.html"/>.
    /// </summary>
    public class CredentialsManager
    {
        private string _gameName = string.Empty;
        private string _envCode = string.Empty;

        /// <summary>
        /// Combines the gamename, environment code and the 'GameKit' phase to create a profile name 
        /// </summary>
        /// <returns>Profile name that will be used to save/find credentials in the user's AWS credentials ini file</returns>
        private string GetProfileName()
        {
            return $"GameKit-{_gameName}-{_envCode}";
        }

        /// <summary>
        /// Sets the _gameName variable for the instance of the credentials manager
        /// </summary>
        /// <param name="name">The name of the game</param>
        public void SetGameName(string name)
        {
            _gameName = name;
        }

        /// <summary>
        /// Sets the _envCode variable for the instance of the credentials manager
        /// </summary>
        /// <param name="environmentCode">A 2 to 3 character environment code</param>
        public void SetEnv(string environmentCode)
        {
            _envCode = environmentCode;
        }

        /// <summary>
        /// Save a new profile to the AWS credentials file.
        /// </summary>
        /// <param name="accessKey">The access key of the AWS IAM role we are saving.</param>
        /// <param name="secretKey">The secret key of the AWS IAM role we are saving.</param>
        public void SaveCredentials(string accessKey, string secretKey)
        {
            uint result = CoreWrapper.Get().SaveAWSCredentials(GetProfileName(), accessKey, secretKey, Logging.LogCb);

            if (result != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Logging.LogError($"CredentialsManager::SaveCredentials() was unable to save profile: {GetProfileName()} to aws credentials file. Error Code: {result}");
            }
        }

        /// <summary>
        /// Checks the AWS profile exists.
        /// </summary>
        /// <param name="gameName">The game name we are checking an aws profile for.</param>
        /// <param name="environmentCode">The environment code we are checking an aws profile for.</param>
        public bool CheckAwsProfileExists(string gameName, string environmentCode)
        {
            SetGameName(gameName);
            SetEnv(environmentCode);
            return CoreWrapper.Get().AwsProfileExists(GetProfileName());
        }

        /// <summary>
        /// Sets the AWS access key of an existing profile. 
        /// </summary>
        /// <remarks>
        /// If the profile retrieved with GetProfileName does not exist, will not automatically create the profile and will log an error.
        /// </remarks>
        /// <param name="accessKey">The new access key of the AWS IAM role we are saving.</param>
        public void SetAccessKey(string accessKey)
        {
            uint result = CoreWrapper.Get().SetAWSAccessKey(GetProfileName(), accessKey, Logging.LogCb);

            if (result != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Logging.LogError($"CredentialsManager::SetAccessKey() was unable to set access key for profile: {GetProfileName()}. Error Code: {result}");
            }
        }

        /// <summary>
        /// Sets the AWS secret key of an existing profile.
        /// </summary>
        /// <remarks>
        /// If the profile retrieved with GetProfileName does not exist, will not automatically create the profile and will log an error.
        /// </remarks>
        /// <param name="secretKey">The new secret key that will be assigned to this profile.</param>
        public void SetSecretKey(string secretKey)
        {
            uint result = CoreWrapper.Get().SetAWSSecretKey(GetProfileName(), secretKey, Logging.LogCb);

            if (result != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Logging.LogError($"CredentialsManager::SetSecretKey() was unable to set secret key for profile: {GetProfileName()}. Error Code: {result}");
            }
        }

        /// <summary>
        /// Gets the access key corresponding to a pre-existing profile in the AWS credentials file.
        /// </summary>
        /// <returns>The access key found in the .aws/credentials file for the corresponding profile</returns>
        public string GetAccessKey()
        {
            // KeyValueStringCallbackResult is a struct containing two strings ResponseKey and ResponseValue and and error code, which can be parsed in GameKitErrors.cs.
            // In this method ResponseKey corresponds to the Access Key and ResponseValue corresponds to the Secret Key
            KeyValueStringCallbackResult result = CoreWrapper.Get().GetAWSProfile(GetProfileName(), Logging.LogCb);

            if (result.ResultCode != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Logging.LogError($"CredentialsManager::GetAccessKey() was unable to get access key for profile: {GetProfileName()}. Error Code: {result}");
            }

            return result.ResponseKey;
        }

        /// <summary>
        /// Gets the secret key corresponding to a pre-existing profile in the AWS credentials file.
        /// </summary>
        /// <returns>The secret key found in the .aws/credentials file for the corresponding profile</returns>
        public string GetSecretAccessKey()
        {
            // KeyValueStringCallbackResult is a struct containing two strings ResponseKey and ResponseValue and and error code, which can be parsed in GameKitErrors.cs.
            // In this method ResponseKey corresponds to the Access Key and ResponseValue corresponds to the Secret Key
            KeyValueStringCallbackResult result = CoreWrapper.Get().GetAWSProfile(GetProfileName(), Logging.LogCb);

            if (result.ResultCode != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Logging.LogError($"CredentialsManager::GetSecretAccessKey() was unable to get secret key for profile: {GetProfileName()}. Error Code: {result}");
            }

            return result.ResponseValue;
        }
    }
}
