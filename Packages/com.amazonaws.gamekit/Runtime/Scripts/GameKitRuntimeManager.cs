// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime
{
    /// <summary>
    /// This script must always be attached to an active object in the current scene, otherwise the AWS GameKit APIs will not work.
    /// This script automatically creates and attaches itself to a GameObject named "GameKit" if this script isn't present in the scene.
    /// This happens by calling CreateGameKitRuntimeManagerIfNotValid() below during the normal editor window update state.
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent]
    public class GameKitRuntimeManager : MonoBehaviour
    {
        private const string GAME_KIT_OBJECT_NAME = "GameKitManager";

        private static GameKitRuntimeManager _instance;
        private static GameObject _objectInstance;

        private static bool _isActive = false;

#region Editor Code
        /// <summary>
        /// This method will check for and add (if missing) a GameKit game object with this script attached.
        /// </summary>
        /// <param name="objectInstance">A handle for the GameKit object that this method creates.</param>
        public static void KeepGameKitObjectAlive()
        {
            // if the script is already active then there is nothing to do
            if (!_isActive)
            {
                // attempt to get an instance of the object or create a new one
                GameObject instance = GetGameObjectInstance();

                // try to get an instance of this script from the object
                GameKitRuntimeManager scriptInstance = instance.GetComponent<GameKitRuntimeManager>();

                // if the script was not attached, then attach it
                if (scriptInstance == null)
                {
                    scriptInstance = instance.AddComponent<GameKitRuntimeManager>();
                }

                // enable the script
                scriptInstance.enabled = true;

                // the object should not move and always be set to static
                instance.isStatic = true;

                // activate the object
                instance.SetActive(true);
            }
        }

        private static GameObject GetGameObjectInstance()
        {
            // if there is not already an instance of the object then create it
            if (_objectInstance == null)
            { 
                _objectInstance = new GameObject(GAME_KIT_OBJECT_NAME);
            }

            return _objectInstance;
        }
#endregion

#region Runtime Code
        private void Awake()
        {
            if (Application.isPlaying)
            {
                if (_instance == null)
                {
                    _instance = GetComponent<GameKitRuntimeManager>();

                    // prevent the parent of this object from being destroyed during scene changes
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        private void OnEnable()
        {
            Logging.LogInfo("GameKitRuntimeManager is now running.");

            _isActive = true;
        }

        private void Update()
        {
            GameKitManager gameKitManager = Singleton<GameKitManager>.Get();
            gameKitManager.EnsureFeaturesAreInitialized();
            gameKitManager.Update();
        }

        private void OnDisable()
        {
            _isActive = false;
            Logging.LogInfo("GameKitRuntimeManager is no longer running.");
        }

        private void OnApplicationQuit()
        {
#if !UNITY_EDITOR
            // In editor mode don't dispose GameKit manager. 
            // In non-editor execution environments and while we are still on the main thread, 
            // clean up the GameKitManager and related features
            Singleton<GameKitManager>.Get().Dispose();
#endif

            // cleanup the instance
            _instance = null;
        }
        
        private void OnApplicationPause(bool isPaused)
        {
            Singleton<GameKitManager>.Get().OnApplicationPause(isPaused);
        }
#endregion
    }
}
