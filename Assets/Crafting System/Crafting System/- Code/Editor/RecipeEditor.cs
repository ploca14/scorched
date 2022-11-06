using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomEditor(typeof(RecipeObject))]
    public class RecipeEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var ve = new VisualElement();
            var recipe = (RecipeObject) target;
            var serialized = new SerializedObject(recipe);
            var requirementEditor = new PropertyField(serialized.FindProperty("requirements"));
            var outputEditor = new PropertyField(serialized.FindProperty("results"));
            outputEditor.style.marginBottom = 16f;

            ve.Add(new Label("Ingredients"));
            ve.Add(requirementEditor);
            requirementEditor.style.marginBottom = 16f;
            ve.Add(new Label("Result"));
            ve.Add(outputEditor);
            ve.Add(new Label("Categories").CenterContents());
            ve.Add(VisualElementPresets.CreateStandardCategoryEditor(recipe));
            return ve;
        }
    }
}