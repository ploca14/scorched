using Polyperfect.Common;
using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration.UGUI
{
    public class RecipeDisplay : ItemUserBase
    {
        public override string __Usage => "An easy way of displaying items. They are added as children. Created children can be cleared via Unity Events.";
        public ChildConstructor RequirementsConstructor;
        public ChildConstructor OutputConstructor;
        public void DisplayRecipe(RuntimeID recipeID)
        {
            if (RequirementsConstructor)
            {
                RequirementsConstructor.Construct(World.Recipes[recipeID].Requirements,
                        (go, stack) => go.GetComponent<IInsert<ItemStack>>().InsertPossible(stack));
            }

            if (OutputConstructor)
            {
                OutputConstructor.Construct(World.Recipes[recipeID].Output, 
                        (go, stack) => go.GetComponent<IInsert<ItemStack>>().InsertPossible(stack));
                
            }
        }

        public void ClearChildren()
        {
            if (RequirementsConstructor)
                RequirementsConstructor.ClearConstructed();
            if (OutputConstructor)
                OutputConstructor.ClearConstructed();
        }
    }
}