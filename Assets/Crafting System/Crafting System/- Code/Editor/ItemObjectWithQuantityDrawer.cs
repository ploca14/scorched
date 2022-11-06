using System;
using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomPropertyDrawer(typeof(ObjectItemStack))]
    public class ItemObjectWithQuantityDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var ve = new VisualElement();

            PopulateElement(property, ve);
            return ve;
        }

        static void PopulateElement(SerializedProperty property, VisualElement ve)
        {
            ve.Clear();
            var obj = property.FindPropertyRelative("item");
            var count = property.FindPropertyRelative("count");
            ValidateCountData();

            var objField = new PropertyField(obj);
            ve.Add(objField);
            objField.RegisterCallback<ChangeEvent<BaseObjectWithID>>(e =>
            {
                count.intValue = Mathf.Max(1, count.intValue);
                ve.UpdatePropertyField(property);
            });
            var countLabel = new Label(count.intValue.ToString());
            var countField = new PropertyField(count, "\0");

            countField.Hide();
            countLabel.DisplayIf(count.intValue>0);

            var labelClickManipulator = new Clickable(() =>
            {
                countLabel.Hide();
                countField.Show();
                var input = countField.Q("unity-text-input");
                input.Focus();
            });
            var filter = new ManipulatorActivationFilter {button = 0, clickCount = 1};
            labelClickManipulator.activators.Add(filter);
            countLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            countLabel.AddManipulator(labelClickManipulator);
            countLabel.AddManipulator(new MouseHoverManipulator(
                e => countLabel.style.fontSize = 20f,
                e => countLabel.style.fontSize = 12f));
            countField.style.width = 48f;
            ve.Add(countLabel);
            ve.Add(countField);
            ve.schedule.DoubleDelay(() =>
            {
                try
                {
                    //countField.ElementAt(0).ElementAt(0).Hide(); //remove the label
                    countField.HideLabel();
                    countField.RemoveFromHierarchy();
                    var slot = objField.Q("slot");
                    slot.Add(countField);
                    slot.Add(countLabel);
                    countField.Q("unity-text-input")?.RegisterCallback<BlurEvent>(b =>
                    {
                        if (count.intValue == 0)
                            obj.objectReferenceValue = null;

                        ve.UpdatePropertyField(property);
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error encountered while setting up item slot: {e}");
                }
            });

            void ValidateCountData()
            {
                var updated = true;
                if (!obj.objectReferenceValue)
                    count.intValue = 0;
                else if (count.intValue == 0)
                    count.intValue = 1;
                else 
                    updated = false;

                if (updated)
                    property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}