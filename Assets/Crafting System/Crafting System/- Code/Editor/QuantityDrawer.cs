using Polyperfect.Crafting.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomPropertyDrawer(typeof(Quantity))]
    public class QuantityDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new PropertyField(property.FindPropertyRelative(nameof(Quantity.Value)), property.name);
        }
    }
}