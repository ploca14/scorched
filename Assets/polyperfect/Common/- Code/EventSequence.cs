using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Common
{
    public class EventSequence : PolyMono
    {
        public override string __Usage => "A simple sequence of events.";

        [Serializable]
        class DelayedEvent
        {
            public float Delay = 1;
            public UnityEvent Event;
        }

        [SerializeField] List<DelayedEvent> Events;

        IEnumerator Start()
        {
            foreach (var item in Events)
            {
                yield return new WaitForSeconds(item.Delay);
                item.Event.Invoke();
            }
        }
    }
}