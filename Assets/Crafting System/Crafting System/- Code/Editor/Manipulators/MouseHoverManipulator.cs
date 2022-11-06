using System;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    public class MouseHoverManipulator : Manipulator
    {
        readonly Action<MouseEnterEvent> _onEnter;
        readonly Action<MouseLeaveEvent> _onLeave;

        public MouseHoverManipulator(Action<MouseEnterEvent> onEnter, Action<MouseLeaveEvent> onLeave)
        {
            _onEnter = onEnter;
            _onLeave = onLeave;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(HandleEnter);
            target.RegisterCallback<MouseLeaveEvent>(HandleExit);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(HandleEnter);
            target.UnregisterCallback<MouseLeaveEvent>(HandleExit);
        }

        void HandleEnter(MouseEnterEvent evt)
        {
            _onEnter(evt);
        }

        void HandleExit(MouseLeaveEvent evt)
        {
            _onLeave(evt);
        }
    }
}