using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomEditor(typeof(BaseItemObject))]
    public class ItemObjectEditor : Editor
    {
        readonly Dictionary<VisualElement, Object> associatedRecipeData = new Dictionary<VisualElement, Object>();

        //IReadOnlyDictionary<RuntimeID, IconData> _iconLookup;

        // void OnEnable()
        // {
        //     _iconLookup = EditorItemDatabase.Data.GetReadOnlyAccessor<IconData>();
        // }


        public override VisualElement CreateInspectorGUI()
        {
            var itemTarget = (BaseItemObject) target;
            if (!target)
            {
                Debug.LogError("Null item, aborting");
                return new VisualElement();
            }

            var ve = new VisualElement();
            //var categories = AssetUtility.FindAssetsOfType<BaseCategoryObject>().ToArray();
            var groupControl = VisualElementPresets.CreateStandardCategoryEditor(itemTarget).SetGrow();

            var recipeSection = new VisualElement();
            recipeSection.style.marginTop = 16f;
            recipeSection.SetGrow();

            recipeSection.Add(new Label("Recipes").CenterContents());

            var recipeRow = new VisualElement().SetRow().SetGrow();
            var madeFromSection = new VisualElement();
            madeFromSection.SetGrow();
            var madeFrom = new FilterableListview<BaseRecipeObject>(e => true,
                () => AssetUtility.FindAssetsOfType<BaseRecipeObject>().Where(r => r.Outputs.Select(o => o.ID).Contains(itemTarget)));
            SetupRecipeList(madeFrom);
            var usedInSection = new VisualElement();
            usedInSection.SetGrow();
            var usedIn = new FilterableListview<BaseRecipeObject>(e => true,
                () => AssetUtility.FindAssetsOfType<BaseRecipeObject>().Where(r => r.Ingredients.Select(o => o.ID).Contains(itemTarget)));
            SetupRecipeList(usedIn);

            madeFromSection.Add(new Label("CREATED BY").CenterContents());
            madeFromSection.Add(madeFrom);

            usedInSection.Add(new Label("USED IN").CenterContents());
            usedInSection.Add(usedIn);

            recipeRow.Add(madeFromSection);
            recipeRow.Add(usedInSection);

            recipeSection.Add(recipeRow);

            ve.Add(new Label("Categories").CenterContents());
            ve.Add(groupControl);
            ve.Add(recipeSection);

            return ve;
        }

        void SetupRecipeList(FilterableListview<BaseRecipeObject> listview)
        {
            listview.makeItem = () =>
            {
                var v = new VisualElement();
                v.Add(new Label());

                var openForEditManip = new OpenForEditManipulator(() => associatedRecipeData[v], 1);
                v.AddManipulator(openForEditManip);
                v.AddManipulator(new ClickEater());
                return v;
            };
            listview.bindItem = (element, i) =>
            {
                element.Q<Label>().text = listview.FilteredItems[i].name;
                associatedRecipeData[element] = listview.FilteredItems[i];
            };
            listview.itemHeight = 24;
            listview.style.minHeight = 150f;
            listview.SetMargin(4f);
            listview.SetGrow();
            listview.UpdateOriginals();
        }

        // void HandleIconChange(ChangeEvent<Object> evt)
        // {
        //     var iconObject = AssetUtility.FindAssetsOfType<IconsCategory>().First();
        //     var item = (BaseItemObject) target;
        //     var iconData = new IconData((Sprite) evt.newValue);
        //     iconObject.SetData(item,iconData);
        //     _iconLookup[item] = iconData;
        // }
    }
}