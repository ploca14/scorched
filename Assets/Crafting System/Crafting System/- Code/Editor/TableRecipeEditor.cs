using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomEditor(typeof(TableRecipeObject))]
    public class TableRecipeEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var ve = new VisualElement();

            var tableRecipe = (TableRecipeObject) target;

            var tableSerialized = new SerializedObject(tableRecipe);

            var requirementsEditor = new PropertyField(tableSerialized.FindProperty("requirements"));
            var outputEditor = new PropertyField(tableSerialized.FindProperty("results"));

            var ingredients = new VisualElement();
            ingredients.Add(requirementsEditor);

            var results = new VisualElement();
            results.style.marginTop = 32f;
            results.Add(new Label("Result"));
            results.Add(outputEditor);

            ve.Add(ingredients);
            ve.Add(results);
            ve.Add(VisualElementPresets.CreateStandardCategoryEditor(tableRecipe));

            return ve;
        }
    }
}