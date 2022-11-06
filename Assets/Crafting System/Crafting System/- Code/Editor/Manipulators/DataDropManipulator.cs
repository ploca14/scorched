using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Polyperfect.Crafting.Edit
{
    /// <summary>
    ///     Uses the UnityEditor's Drag and Drop functionality
    /// </summary>
    public class DataDropManipulator<T> : MouseManipulator where T : Object
    {
        readonly Action<T> _onReceived;

        public DataDropManipulator(Action<T> onReceived)
        {
            _onReceived = onReceived;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragPerformEvent>(HandleDragPerform);
            target.RegisterCallback<DragUpdatedEvent>(HandleDragUpdated);
        }

        void HandleDragUpdated(DragUpdatedEvent evt)
        {
            if (!CanDrag())
                return;
            
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragPerformEvent>(HandleDragPerform);
            target.UnregisterCallback<DragUpdatedEvent>(HandleDragUpdated);
        }

        void HandleDragPerform(DragPerformEvent evt)
        {
            if (!CanDrag())
                return;
            _onReceived.Invoke((T) DragAndDrop.objectReferences.First());
            DragAndDrop.AcceptDrag();
        }

        static bool CanDrag()
        {
            return !(DragAndDrop.objectReferences.Length <= 0 || !(DragAndDrop.objectReferences.First() is T));
        }
    }
}