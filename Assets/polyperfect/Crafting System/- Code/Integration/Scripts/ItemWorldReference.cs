using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    [DefaultExecutionOrder(-999)]
    public class ItemWorldReference : PolyMono
    {
        public override string __Usage => $"Determines which {nameof(ItemWorldFragment)} is used in the scene.";

        public IItemWorld World;
        static ItemWorldReference instance;
        [SerializeField] ItemWorldFragment[] UsedFragments;


        public static ItemWorldReference Instance
        {
            get
            {
                if (!instance)
                    throw new Exception(
                        $"There is no {nameof(ItemWorldReference)} in the scene, or a script is trying to access it before it has been initialized. Please create one in Edit mode.");
                return instance;
            }
        }

        void Awake()
        {
            instance = this;
            var mainWorld = new SimpleItemWorld();
            RuntimeID.SettableStaticNameLookup = mainWorld.GetNameLookup();
            foreach (var world in UsedFragments)
            {
                foreach (var itemObject in world.ItemObjects)
                {
                    mainWorld.AddItem(itemObject.ID, itemObject.name);
                }

                foreach (var categoryObject in world.CategoryObjects)
                {
                    if (categoryObject is BaseCategoryWithData withData)
                        mainWorld.AddCategoryWithData(categoryObject, categoryObject.name, withData.ConstructDictionary());
                    else
                        mainWorld.AddCategory(categoryObject, categoryObject.name,categoryObject.ValidMembers.Select(m=>m.ID));
                }

                foreach (var recipeObject in world.RecipeObjects)
                {
                    mainWorld.AddRecipe(recipeObject.ID,new SimpleRecipe(recipeObject.Ingredients,recipeObject.Outputs),recipeObject.name);
                }
            }

            World = mainWorld;
        }
    }
}