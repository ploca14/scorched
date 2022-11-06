using System;
using Polyperfect.Common;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Polyperfect.Crafting.Integration.UGUI
{
    public class UGUIHoverEvents : PolyMono,IPointerEnterHandler,IPointerExitHandler
    {
        public UnityEvent OnHoverBegin,OnHoverEnd;
        public override string __Usage => "Listens only for PointerEnter and PointerExit events.";
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverBegin.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverEnd.Invoke();
        }
    }
}