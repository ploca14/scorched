using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    public class DirtyOnChangeManipulator<T> : Manipulator
    {
        readonly Object _obj;

        public DirtyOnChangeManipulator(Object obj)
        {
            _obj = obj;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<ChangeEvent<T>>(HandleChange);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<ChangeEvent<T>>(HandleChange);
        }

        void HandleChange(ChangeEvent<T> evt)
        {
            EditorUtility.SetDirty(_obj);
        }
    }
}