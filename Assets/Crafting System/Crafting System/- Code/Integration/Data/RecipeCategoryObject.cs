using System;

namespace Polyperfect.Crafting.Integration
{
    [CreateMenuTitle("For Recipes")]
    public class RecipeCategoryObject : CategoryObject
    {
        public override Func<BaseObjectWithID, bool> Criteria => o => o is BaseRecipeObject;
    }
}