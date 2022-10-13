using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    /// <summary>
    ///     Consumes click events.
    /// </summary>
    public class ClickEater : Manipulator
    {
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(HandleMouseDown);
        }

        void HandleMouseDown(MouseDownEvent evt)
        {
            evt.StopImmediatePropagation();
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(HandleMouseDown);
        }
    }
}