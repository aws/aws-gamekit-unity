// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Threading;

#if UNITY_EDITOR
// Unity
using UnityEditor;
#endif

namespace AWS.GameKit.Runtime.Utils
{
    /// <summary>
    /// Utility class that wraps the calls to functions with a predefined signature inside of a thread and then manages the callbacks resulting from the calls.<br/><br/> 
    /// 
    /// Callbacks are called by using the Threader class's Update() method. This method should be called from one of Unity's Update() lifecycle methods. 
    /// This will result in all callbacks being executed by Unity's main thread, thus making Unity calls from the callbacks safe.  
    /// </summary>
    public class Threader
    {
        /// <summary>
        /// This is the count of how many callbacks are currently waiting to be executed but have not been loaded into the main thread safe queue
        /// </summary>
        public int WaitingQueueCount => _waitingQueue.Count;

        /// <summary>
        /// This is the count of how many callbacks are currently waiting to be executed and are in the queue to be executed on the main thread
        /// </summary>
        public int ExecutionQueueCount => _executionQueue.Count;

        // Queue of callbacks waiting to be called - touched by both the functions thread and main thread
        private List<Action> _waitingQueue = new List<Action>();

        // Queue of callbacks in the process of being called - touched only by the main thread
        private List<Action> _executionQueue = new List<Action>();

        // Internal generational counter which increments when Awake is called - callbacks queued up from previous generations are discarded
        private int _generationCounter = 0;

        // Internal count of threaded work items outstanding - incremented on main thread, decremented on work thread
        private int _atomicOutstandingWorkCounter = 0;
         
        // Internal identifier for this threader
        private int _threaderId => GetHashCode();

        /// <summary>
        /// Calls, in its own thread, a simple function that returns a RESULT object<br/><br/>
        /// 
        /// RESULT: An object signature for the output object returned by the function Threader will act on
        /// </summary>
        /// <param name="function">The function to wrap in a thread, signature must be RESULT function(void)</param>
        /// <param name="callback">Action that is called on the completion of the thread</param>
        public virtual void Call(Action function, Action callback)
        {
            // lambda captures object references, including "this"; explicitly capture counter value
            int initialCounter = _generationCounter;
            IncrementWorkCounterAndRun(() =>
            {
                function();
                AddToWaitingQueue(initialCounter, callback);
                DecrementWorkCounter();
            });
        }

        /// <summary>
        /// Calls, in its own thread, a simple function that returns a RESULT object<br/><br/>
        /// 
        /// RESULT: An object signature for the output object returned by the function Threader will act on
        /// </summary>
        /// <param name="function">The function to wrap in a thread, signature must be RESULT function(void)</param>
        /// <param name="callback">Action that takes a RESULT type object and is called on the completion of the thread</param>
        public virtual void Call<RESULT>(Func<RESULT> function, Action<RESULT> callback)
        {
            // lambda captures object references, including "this"; explicitly capture counter value
            int initialCounter = _generationCounter;
            IncrementWorkCounterAndRun(() =>
            {
                RESULT result = function();
                AddToWaitingQueue(initialCounter, callback, result);
                DecrementWorkCounter();
            });
        }

        /// <summary>
        /// Calls, in its own thread, a simple function that takes a DESCRIPTION object and returns a RESULT object<br/><br/>
        /// 
        /// DESCRIPTION: An object signature for the input object sent into the function Threader will act on<br/>
        /// RESULT: An object signature for the output object returned by the function Threader will act on
        /// </summary>
        /// <param name="function">The function to wrap in a thread, signature must be RESULT function(DESCRIPTION)</param>
        /// <param name="description">Object provided the the wrapped function when it is called</param>
        /// <param name="callback">Action that takes a RESULT type object and is called on the completion of the thread</param>
        public virtual void Call<DESCRIPTION, RESULT>(Func<DESCRIPTION, RESULT> function, DESCRIPTION description, Action<RESULT> callback)
        {
            // lambda captures object references, including "this"; explicitly capture counter by-value
            int initialCounter = _generationCounter;
            IncrementWorkCounterAndRun(() =>
            {
                RESULT result = function(description);
                AddToWaitingQueue(initialCounter, callback, result);
                DecrementWorkCounter();
            });
        }

        /// <summary>
        /// Calls, in its own thread, a function that calls multiple callbacks, such as a paginated function. The function must take a DESCRIPTION and callback then return void<br/><br/>
        /// 
        /// DESCRIPTION: An object signature for the input object sent into the function Threader will act on<br/>
        /// RESULT: An object signature for the output object returned by the function Threader will act on
        /// </summary>
        /// <param name="function">The function to wrap in a thread, signature must be void function(DESCRIPTION, Action&lt;RESULT&gt;)</param>
        /// <param name="description">Object provided the the wrapped function when it is called</param>
        /// <param name="callback">Action that takes a RESULT type object and is called when the wrapped function has a result to return</param>
        public virtual void Call<DESCRIPTION, RESULT, RETURN_RESULT>(
            Func<DESCRIPTION, Action<RESULT>, RETURN_RESULT> function,
            DESCRIPTION description,
            Action<RESULT> callback,
            Action<RETURN_RESULT> onCompleteCallback)
        {
            // lambda captures object references, including "this"; explicitly capture counter by-value
            int initialCounter = _generationCounter;
            IncrementWorkCounterAndRun(() =>
            {
                Action<RESULT> wrappedCallback = (RESULT result) => { AddToWaitingQueue(initialCounter, callback, result); };
                RETURN_RESULT result = function(description, wrappedCallback);
                AddToWaitingQueue(initialCounter, onCompleteCallback, result);
                DecrementWorkCounter();
            });
        }

        /// <summary>
        /// Calls, in its own thread, a simple function that takes a DESCRIPTION object and returns nothing<br/><br/>
        /// 
        /// DESCRIPTION: An object signature for the input object sent into the function Threader will act on
        /// </summary>
        /// <param name="function">The function to wrap in a thread, signature must be void function(DESCRIPTION)</param>
        /// <param name="description">Object provided the the wrapped function when it is called</param>
        /// <param name="callback">Action that takes a RESULT type object and is called on the completion of the thread</param>
        public virtual void Call<DESCRIPTION>(Action<DESCRIPTION> function, DESCRIPTION description, Action callback)
        {
            // lambda captures object references, including "this"; explicitly capture counter by-value
            int initialCounter = _generationCounter;
            IncrementWorkCounterAndRun(() =>
            {
                function(description);
                AddToWaitingQueue(initialCounter, callback);
                DecrementWorkCounter();
            });
        }

        /// <summary>
        /// Called to (re)initialize the threader when changing play modes or scenes
        /// </summary>
        public virtual void Awake()
        {
            Logging.LogInfo($"GameKit Threader {_threaderId} Awake");

            // clear the queues each time this class is used to prevent any spill over when changing from playmode to editmode (and visa versa)
            lock (_waitingQueue)
            {
                _waitingQueue.Clear();
                _executionQueue.Clear();
                _generationCounter = unchecked(_generationCounter + 1); // overflow is possible, ignored.
            }
        }

        /// <summary>
        /// Called to handle any callbacks generated by the called functions
        /// </summary>
        public virtual void Update()
        {
            // move any actions waiting to execute into the execture queue
            lock (_waitingQueue)
            {
                _executionQueue.AddRange(_waitingQueue);
                _waitingQueue.Clear();
            }

            // execute the current actions
            try
            {
                _executionQueue.ForEach((action) =>
                {
                    action();

#if UNITY_EDITOR
                    // if there has been a callback, request that our settings window updates so that any results from that callback can display if needed
                    SettingsWindowUpdateController.RequestUpdate();
#endif
                });
            }
            finally
            {
                _executionQueue.Clear();
            }
        }

        /// <summary>
        /// Waits for the completion of pending tasks in a loop (blocking). No-op in Editor.
        /// </summary>
        /// <param name="loopDelay">Time to wait before checking for task completion in each loop iteration.</param>
        public void WaitForThreadedWork(int loopDelay = 50)
        {
#if !UNITY_EDITOR
            // In game mode wait for pending tasks
            WaitForThreadedWork_Internal(loopDelay);
#endif
        }

        /// <summary>
        /// Waits for the completion of pending tasks in a loop (blocking).
        /// </summary>
        /// <param name="loopDelay">Time to wait before checking for task completion in each loop iteration.</param>
        private void WaitForThreadedWork_Internal(int loopDelay = 50)
        {
            while (true)
            {
                // Note: slightly abusing Interlocked.Add(x, 0) as an atomic read w/ full memory barrier.
                if (Interlocked.Add(ref _atomicOutstandingWorkCounter, 0) == 0)
                {
                    Logging.LogInfo($"GameKit Threader {_threaderId} WaitForThreadedWork: No outstanding work remaining.");
                    return;
                }

                Logging.LogInfo($"GameKit Threader {_threaderId} WaitForThreadedWork: Waiting for outstanding work...");
                Thread.Sleep(loopDelay);
            }
        }

        /// <summary>
        /// Called to block until threaded work is complete. Callbacks remain unprocessed in the wait queue. Intended for tests only.
        /// </summary>
        public virtual void WaitForThreadedWork_TestOnly()
        {
            WaitForThreadedWork_Internal(1);
        }

        /// <summary>
        /// Helper method for adding a new callback action to the waiting queue in preparation for it to be called by the main thread.
        /// </summary>
        /// <param name="checkCounter">Generational counter value when call was initiated - callback will be ignored if counter has changed</param>
        /// <param name="callback">Callback to execute on the main thread during the update game state. Must have signature void func(RESULT)</param>
        /// <param name="result">Object that is passed into the callback function when it is executed</param>
        private void AddToWaitingQueue<RESULT>(int checkCounter, Action<RESULT> callback, RESULT result)
        {
            lock (_waitingQueue)
            {
                if (checkCounter == _generationCounter)
                {
                    _waitingQueue.Add(() => callback(result));
                }
            }
        }

        /// <summary>
        /// Helper method for adding a new callback action to the waiting queue in preparation for it to be called by the main thread.
        /// </summary>
        /// <param name="checkCounter">Generational counter value when call was initiated - callback will be ignored if counter has changed</param>
        /// <param name="callback">Callback to execute on the main thread during the update game state. Must have signature void func(void)</param>
        private void AddToWaitingQueue(int checkCounter, Action callback)
        {
            lock (_waitingQueue)
            {
                if (checkCounter == _generationCounter)
                {
                    _waitingQueue.Add(callback);
                }
            }
        }

        /// <summary>
        /// Helper method for tracking when threaded work item has completed (adjusting _atomicOutstandingWorkCounter)
        /// </summary>
        private void DecrementWorkCounter()
        {
            Interlocked.Decrement(ref _atomicOutstandingWorkCounter);
        }

        /// <summary>
        /// Helper method for scheduling an async threaded action from the main thread (adjusting _atomicOutstandingWorkCounter)
        /// </summary>
        /// <param name="action">Action to be executed asynchronously</param>
        private void IncrementWorkCounterAndRun(Action action)
        {
            Interlocked.Increment(ref _atomicOutstandingWorkCounter);

            bool queued = false;
            try
            {
                queued = ThreadPool.QueueUserWorkItem(delegate (object param) { ((Action)param)(); }, action);
            }
            finally
            {
                // If QueueUserWorkItem throws for any reason, do not leave the counter permanently incremented
                if (!queued)
                {
                    DecrementWorkCounter();
                }
            }
        }
    }
}
