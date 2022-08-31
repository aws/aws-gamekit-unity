// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Gamekit
using AWS.GameKit.Common;
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.FeatureUtils
{
    /// <summary>
    /// Wrapper around a singleton for defining a GameKitFeature, for use when calling GameKit features within code
    /// </summary>
    public abstract class GameKitFeature<T> : Singleton<T> where T : GameKitFeatureBase, new() { }

    /// <summary>
    /// Parent class required by all GameKit features
    /// </summary>
    public abstract class GameKitFeatureBase
    {
        // declare the state of this instance of the feature
        public bool IsReady => _isReady;

        public abstract FeatureType FeatureType { get; }

        protected Threader _threader = new Threader();

        private bool _isReady = false;

        /// <summary>
        /// Called by the GameKitManager to initialize any requirements of this feature. Called when the <see cref="GameKitManager.EnsureFeaturesAreInitialized"/> is called.
        /// </summary>
        public virtual void OnInitialize()
        {
            // initialize the threader
            _threader.Awake();

            InitializeFeature();

            // declare the state of the singleton as ready
            _isReady = true;
        }

        /// <summary>
        /// Called by the GameKitManager to clean up the feature. Called when the <see cref="GameKitManager.Dispose"/> is called.
        /// </summary>
        public virtual void OnDispose()
        {
            // Wait for Threader's pending tasks
            _threader.WaitForThreadedWork();

            // declare the state of the singleton as not ready
            _isReady = false;

            // handle any per case requirements before release the feature
            DestroyFeature();

            // release the feature
            GetFeatureWrapperBase().Release();
        }
        
        /// <summary>
        /// Called by the GameKitManager to notify features the game has been paused. Called when <see cref="GameKitManager.OnApplicationPause"/> is called.
        /// </summary>
        /// <param name="isPaused">True if the application is paused, else False.</param>
        public virtual void OnApplicationPause(bool isPaused)
        {
            if (_isReady)
            {
                NotifyPause(isPaused);
            }
        }

        /// <summary>
        /// Called by the GameKitManager to update any requirements of the feature. Called when <see cref="GameKitManager.Update"/> is called.
        /// </summary>
        public virtual void Update()
        {
            if (_isReady)
            {
                // call any feature specific updates
                UpdateFeature();

                // call all queued callbacks
                _threader.Update();
            }
        }

        /// <summary>
        /// For use within a GameKitFeature for calling a feature's API and wrapping inside of a threader call. Also validates if this feature has been initialized.
        /// </summary>
        /// <param name="function">GameKit feature API to wrap inside of the thread</param>
        /// <param name="callback">Action to call once the thread has completed</param>
        protected void Call(Action function, Action callback)
        {
            if (_isReady)
            {
                _threader.Call(function, callback);
            }
            else
            {
                LogCallNotInitialized();
            }
        }

        /// <summary>
        /// For use within a GameKitFeature for calling a feature's API and wrapping inside of a threader call. Also validates if this feature has been initialized.
        /// </summary>
        /// <param name="function">GameKit feature API to wrap inside of the thread</param>
        /// <param name="callback">Action to call once the thread has completed</param>
        protected void Call<RESULT>(Func<RESULT> function, Action<RESULT> callback)
        {
            if (_isReady)
            {
                _threader.Call(function, callback);
            }
            else
            {
                LogCallNotInitialized();
            }
        }

        /// <summary>
        /// For use within a GameKitFeature for calling a feature's API and wrapping inside of a threader call. Also validates if this feature has been initialized.
        /// </summary>
        /// <param name="function">GameKit feature API to wrap inside of the thread</param>
        /// <param name="description">DESCRIPTION object required to call the API</param>
        /// <param name="callback">Action to call once the thread has completed</param>
        protected void Call<DESCRIPTION, RESULT>(Func<DESCRIPTION, RESULT> function, DESCRIPTION description, Action<RESULT> callback)
        {
            if (_isReady)
            {
                _threader.Call(function, description, callback);
            }
            else
            {
                LogCallNotInitialized();
            }
        }

        /// <summary>
        /// For use within a GameKitFeature for calling a feature's API and wrapping inside of a threader call. Also validates if this feature has been initialized.
        /// </summary>
        /// <param name="function">GameKit feature API to wrap inside of the thread</param>
        /// <param name="description">DESCRIPTION object required to call the API</param>
        /// <param name="callback">Action to call whenever the function needs to return information to the caller</param>
        /// <param name="onCompleteCallback">Action to call on the completion of this method</param>
        protected void Call<DESCRIPTION, RESULT, RETURN_RESULT>(
            Func<DESCRIPTION, Action<RESULT>, RETURN_RESULT> function,
            DESCRIPTION description,
            Action<RESULT> callback,
            Action<RETURN_RESULT> onCompleteCallback)
        {
            if (_isReady)
            {
                _threader.Call(function, description, callback, onCompleteCallback);
            }
            else
            {
                LogCallNotInitialized();
            }
        }

        /// <summary>
        /// For use within a GameKitFeature for calling a feature's API and wrapping inside of a threader call. Also validates if this feature has been initialized.
        /// </summary>
        /// <param name="function">GameKit feature API to wrap inside of the thread</param>
        /// <param name="description">DESCRIPTION object required to call the API</param>
        /// <param name="callback">Action to call once the thread has completed</param>
        protected void Call<DESCRIPTION>(Action<DESCRIPTION> function, DESCRIPTION description, Action callback)
        {
            if (_isReady)
            {
                _threader.Call(function, description, callback);
            }
            else
            {
                LogCallNotInitialized();
            }
        }

        /// <summary>
        /// InitializeFeature is called during the Awake state and is an optional call for the child feature.
        /// 
        /// This method does nothing by default. It is not necessary to call `base.InitializeFeature()` when overriding this method.
        /// </summary>
        protected virtual void InitializeFeature()
        {
            // default empty InitializeFeature() call
        }
        
        /// <summary>
        /// NotifyPause is called whenever the application is paused or unpaused. This can be useful on platforms such as iOS where the application is
        /// suspended before being paused and shutdown code can be unreliable.  
        ///
        /// This method does nothing by default. It is not necessary to call `base.NotifyPause()` when overriding this method.
        /// <param name="isPaused"></param>
        /// </summary>
        protected virtual void NotifyPause(bool isPaused)
        {
            // default empty NotifyPause() call
        }

        /// <summary>
        /// UpdateFeature is called during the Update state and is an optional call for the child feature.
        /// 
        /// This method does nothing by default. It is not necessary to call `base.UpdateFeature()` when overriding this method.
        /// </summary>
        protected virtual void UpdateFeature()
        {
            // default empty UpdateFeature() call
        }

        /// <summary>
        /// DestroyFeature is called during the OnDestroy state and is an optional call for the child feature.
        /// 
        /// This method does nothing by default. It is not necessary to call `base.DestroyFeature()` when overriding this method.
        /// </summary>
        protected virtual void DestroyFeature()
        {
            // default empty DestroyFeature() call
        }

        protected abstract GameKitFeatureWrapperBase GetFeatureWrapperBase();

        /// <summary>
        /// Helper method for generating a formatted error log when the related child feature is not initialized
        /// </summary>
        private void LogCallNotInitialized()
        {
            string featureMethod = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().Name;
            string featureClass = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().DeclaringType.FullName;
            string callingMethod = new System.Diagnostics.StackTrace().GetFrame(3).GetMethod().Name;
            string callingClass = new System.Diagnostics.StackTrace().GetFrame(3).GetMethod().DeclaringType.FullName;
            Logging.LogError(
                $"The {featureMethod} method of the {featureClass} feature, called in the {callingMethod} method of {callingClass} class" +
                $", has not been initialized yet. Please make sure {typeof(GameKitManager).FullName} is attached as a component and enabled.");
        }
    }

    /// <summary>
    /// Abstraction layer around the assignment of the feature wrapper
    /// </summary>
    public abstract class GameKitFeatureBase<T> : GameKitFeatureBase where T : GameKitFeatureWrapperBase, new()
    {
        public T Feature => _feature;
        private T _feature = new T();

        protected override GameKitFeatureWrapperBase GetFeatureWrapperBase() => _feature;    
    }
}
