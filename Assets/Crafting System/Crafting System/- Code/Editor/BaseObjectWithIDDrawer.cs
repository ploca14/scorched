using System;
using System.Linq;
using System.Reflection;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomPropertyDrawer(typeof(BaseCategoryObject), true)]
    public class BaseCategoryObjectDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var type = fieldInfo.FieldType;
            while (type.IsGenericType)
            {
                type = type.GetGenericArguments().FirstOrDefault();
                if (type == null)
                {
                    Debug.LogError("Null type when traversing generics. Please report a bug.");
                    return new Label("Error. Check the Console.");
                }
            }
            return new ObjectField() { label = property.displayName, bindingPath = property.propertyPath,objectType = type};
        }
    }
    [CustomPropertyDrawer(typeof(BaseObjectWithID), true)]
    public class BaseObjectWithIDDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var obj = (BaseObjectWithID) property.objectReferenceValue; 
            var icon = EditorItemDatabase.Data.GetReadOnlyAccessor<IconData>().GetDataOrDefault(obj);
            var ve = new VisualElement();
            if (!property.propertyPath.Contains("Array"))
                ve.Add(new Label(property.name));
            ve.style.overflow = Overflow.Visible;
            var slot = new VisualElement {name = "slot"}.SetRow();
            slot.SetRadius(4f);
            slot.SetMargin(4f);
            slot.style.justifyContent = Justify.SpaceBetween;
            slot.style.flexDirection = FlexDirection.RowReverse;
            slot.style.backgroundImage = icon.Icon ? icon.Icon.texture : null;
            slot.style.backgroundColor = new Color(0f, 0f, 0f, .2f);
            slot.style.width = 48f;
            slot.style.height = 48f;
            var deleteButton = new Button(() =>
            {
                var last = property.objectReferenceValue as BaseObjectWithID;
                property.objectReferenceValue = null;

                var propertyField = ve.UpdatePropertyField(property);

                using (var evt = ChangeEvent<BaseObjectWithID>.GetPooled(last, null))
                {
                    evt.target = propertyField;
                    propertyField.SendEvent(evt);
                }
            }) {text = "X"};
            deleteButton.RemoveFromClassList("unity-button");
            if (!icon.Icon && obj)
                slot.Add(new Label(obj.name.NewlineSpaces()).SetStretch().CenterContents());

            slot.AddManipulator(new DataDropManipulator<BaseObjectWithID>(d =>
            {
                var last = property.objectReferenceValue as BaseObjectWithID;
                property.objectReferenceValue = d;

                var propertyField = ve.UpdatePropertyField(property);

                using (var evt = ChangeEvent<BaseObjectWithID>.GetPooled(last, d))
                {
                    evt.target = propertyField;
                    propertyField.SendEvent(evt);
                }
            }));
            var name = obj ? obj.name : "";
            slot.Add(VisualElementPresets.CreateHoverText(slot, name.NewlineSpaces()));
            if (obj)
                slot.Add(deleteButton);
            ve.Add(slot);
            return ve;
        }
    }
}