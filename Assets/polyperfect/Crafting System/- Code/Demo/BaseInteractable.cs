using System;
using Polyperfect.Common;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Demo
{
    public class BaseInteractable : PolyMono
    {
        public override string __Usage => "A generic script for player interactions.";

        [Serializable]
        public class InteractorEvent : UnityEvent<GameObject> { }

        public InteractorEvent OnBeginInteract, OnEndInteract;

        public virtual void BeginInteract(GameObject interactor) => OnBeginInteract.Invoke(interactor);

        public virtual void EndInteract(GameObject interactor) => OnEndInteract.Invoke(interactor);

    }
}