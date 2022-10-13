using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomPropertyDrawer(typeof(Serialized2DItemList))]
    public class Serialized2DItemListEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var ve = new VisualElement();
            
            var elementArray = property.FindPropertyRelative("list");
            var dims = property.FindPropertyRelative("dimensions");
            var iterator = elementArray.Copy();

            var dimsField = new PropertyField(dims);
            dimsField.HideLabel();
            dimsField.style.marginBottom = 16f;
            ve.Add(dimsField);

            var currentDimensions = dims.vector2IntValue;
            if (elementArray.arraySize != currentDimensions.x * currentDimensions.y)
            {
                UpdateArrayDimensions();
                return ve;
            }

            iterator.NextVisible(true);
            iterator.NextVisible(true);
            for (var y = 0; y < currentDimensions.y; y++)
            {
                var row = new VisualElement();
                row.SetRow();
                for (var x = 0; x < currentDimensions.x; x++)
                {
                    row.Add(new PropertyField(iterator.Copy()));
                    iterator.NextVisible(false);
                }

                ve.Add(row);
            }

            //the delay is to prevent an update loop
            dimsField.schedule.Execute(() => dimsField.RegisterCallback<ChangeEvent<Vector2Int>>(e => { UpdateArrayDimensions(); }));

            return ve;

            void UpdateArrayDimensions()
            {
                if (dims.vector2IntValue.x < 1)
                    dims.vector2Value = new Vector2Int(1, dims.vector2IntValue.y);
                if (dims.vector2IntValue.y < 1)
                    dims.vector2Value = new Vector2Int(dims.vector2IntValue.x, 1);
                
                var numberOfElements = dims.vector2IntValue.x * dims.vector2IntValue.y;
                
                elementArray.arraySize = numberOfElements;
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                ve.UpdatePropertyField(property);
            }
        }
    }
}