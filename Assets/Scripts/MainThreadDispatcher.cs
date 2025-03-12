using System;
using System.Threading;

using UnityEngine;

namespace Assets.Scripts
{
    public abstract class MainThreadDispatcher : MonoBehaviour
    {
        private static SynchronizationContext _mainThreadContext;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Save the current SynchronizationContext for the main thread
            _mainThreadContext = SynchronizationContext.Current;
        }

        public static void RunOnMainThread(Action action)
        {
            if (_mainThreadContext == null)
            {
                Debug.LogError("SynchronizationContext not initialized. Make sure it is set up correctly.");
                return;
            }

            _mainThreadContext.Post(_ => action(), null);
        }
    }
}
