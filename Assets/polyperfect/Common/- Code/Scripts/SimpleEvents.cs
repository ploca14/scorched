using System;
using UnityEngine.Events;

namespace Polyperfect.Common
{
    public class SimpleEvents : PolyMono
    {
        public override string __Usage => "Callbacks for standard events.";
        public UnityEvent OnStart, OnActive, OnInactive, OnDestroyed;
        bool applicationIsRunning = true;


        void Awake()
        {
            OnStart = OnStart ?? new UnityEvent();
            OnActive = OnActive ?? new UnityEvent();
            OnInactive = OnInactive ?? new UnityEvent();
            OnDestroyed = OnDestroyed ?? new UnityEvent();
            
#if UNITY_EDITOR
            //check to keep events from being called and potentially spawning objects when destroying
            UnityEditor.EditorApplication.playModeStateChanged += p =>
            {
                if (p == UnityEditor.PlayModeStateChange.ExitingPlayMode)   
                    applicationIsRunning = false;
            };
#endif
        }
        void Start() => OnStart.Invoke();
        void OnEnable() => OnActive.Invoke();
        void OnDisable()
        {
            if (applicationIsRunning)
                OnInactive.Invoke();
        }

        void OnDestroy()
        {
            if (applicationIsRunning)
                OnDestroyed.Invoke();
        }
    }
}