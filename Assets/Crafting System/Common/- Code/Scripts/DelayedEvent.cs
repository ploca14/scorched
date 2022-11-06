using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Common
{
    public class DelayedEvent : PolyMono
    {
        public override string __Usage => "Sends an event after a certain delay.";
        public float Duration = 1f;
        Coroutine activeCoroutine;
        [Serializable]
        public class FloatEvent : UnityEvent<float>
        {
        }

        public UnityEvent OnBegin = new UnityEvent(), OnFinished = new UnityEvent(),OnCancelled = new UnityEvent();
        public FloatEvent OnProgressUpdate = new FloatEvent();
        bool inProgress = false;
        
        public void StartEvent()
        {
            if (!inProgress)
                activeCoroutine = StartCoroutine(EventCoroutine(Duration));
        }

        public void CancelEvent()
        {
            if (inProgress)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
                inProgress = false;
                OnProgressUpdate.Invoke(0f);
                OnCancelled.Invoke();
            }
        }

        void OnDisable()
        {
            CancelEvent();
        }

        IEnumerator EventCoroutine(float duration)
        {
            inProgress = true;
            var startTime = Time.time;
            OnBegin.Invoke();
            var progress = 0f;
            while (progress<1f)
            {
                OnProgressUpdate.Invoke(progress);
                yield return null;
                progress = (Time.time - startTime) / duration;
            }

            inProgress = false;
            progress = 1f;
            OnProgressUpdate.Invoke(progress);
            OnFinished.Invoke();
        }
    }
}