using System;
using Polyperfect.Common;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    public class HoverLabel : Label
    {
        readonly MouseHoverManipulator manipulator;
        readonly VisualElement target;

        public HoverLabel(VisualElement hoverTarget, string text) : base(text)
        {
            target = hoverTarget;
            manipulator = new MouseHoverManipulator(_ =>
            {
                this.Show();
                OnEnter?.Invoke();
            }, _ =>
            {
                this.Hide();
                OnExit?.Invoke();
            });
            RegisterCallback<AttachToPanelEvent>(HandleAttach);
            RegisterCallback<DetachFromPanelEvent>(HandleDetach);
            ;
        }

        public event Action OnEnter, OnExit;

        void HandleAttach(AttachToPanelEvent evt)
        {
            target.AddManipulator(manipulator);
        }

        void HandleDetach(DetachFromPanelEvent evt)
        {
            target.RemoveManipulator(manipulator);
        }
    }
}