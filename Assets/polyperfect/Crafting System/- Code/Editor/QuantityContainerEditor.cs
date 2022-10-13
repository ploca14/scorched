using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomPropertyDrawer(typeof(QuantityContainer))]
    public class QuantityContainerEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var ve = new VisualElement();
            var elementArray = property.FindPropertyRelative("items");
            var iterator = elementArray.Copy();
            var row = new VisualElement().SetRow();
            iterator.NextVisible(true);
            iterator.NextVisible(true);

            for (var x = 0; x < elementArray.arraySize; x++)
            {
                var column = new VisualElement();
                var noHoist = x;
                column.Add(new PropertyField(iterator.Copy()));

                column.Add(new Button(() =>
                {
                    elementArray.DeleteArrayElementAtIndex(noHoist);
                    ve.UpdatePropertyField(elementArray);
                }) {text = "X"});
                row.Add(column);
                iterator.NextVisible(false);
            }

            var addButton = new Button(() =>
            {
                elementArray.arraySize++;
                ve.UpdatePropertyField(elementArray);
            }) {text = "+"};

            addButton.style.fontSize = 18f;
            addButton.AddManipulator(new DataDropManipulator<BaseObjectWithID>(HandleObjectDrop));
            row.Add(addButton);
            ve.Add(row);

            return ve;

            void HandleObjectDrop(BaseObjectWithID obj)
            {
                elementArray.arraySize++;
                var arrayElementAtIndex = elementArray.GetArrayElementAtIndex(elementArray.arraySize - 1);
                arrayElementAtIndex.FindPropertyRelative("item").objectReferenceValue = obj;
                arrayElementAtIndex.FindPropertyRelative("count").intValue = 1;
                ve.UpdatePropertyField(elementArray);
            }
        }
    }
}