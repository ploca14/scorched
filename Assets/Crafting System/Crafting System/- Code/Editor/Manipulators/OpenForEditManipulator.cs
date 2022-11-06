using System;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Polyperfect.Crafting.Edit
{
    public class OpenForEditManipulator : Clickable
    {
        public OpenForEditManipulator(System.Func<Object> getObject, int clickCount, Action refreshAction = null) : base(() =>
            ObjectEditWindow.CreateForObject(getObject(), refreshAction))
        {
            activators.Clear();
            activators.Add(new ManipulatorActivationFilter {button = MouseButton.LeftMouse, clickCount = clickCount});
        }
    }
}