using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Polyperfect.Common
{
    public class ObjectCursor : BaseInstantiatingCursor
    {
        public override string __Usage => "Registers with an InteractiveCursor and allows showing a cursor centered on an object. Sized based on child renderers";
        [Multiline] public string DisplayText = "Interactive Object";
        float scale;
        void Awake()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            var bounds = new Bounds(transform.position, Vector3.one * .1f);
            foreach (var item in renderers)
            {
                bounds.Encapsulate(item.bounds);
            }
            var size = bounds.size;
            scale = new Vector2(size.x, size.z).magnitude;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            
            var childText = instantiated.GetComponentInChildren<Text>();
            if (childText)
                childText.text = string.Format(DisplayText);
        }

        protected override void UpdateCursorPosition(RaycastResult obj)
        {
            var trans = transform;
            instantiated.transform.position = trans.position;
            instantiated.transform.rotation = trans.rotation;
            instantiated.transform.localScale = scale*Vector3.one;
        }
    }
}