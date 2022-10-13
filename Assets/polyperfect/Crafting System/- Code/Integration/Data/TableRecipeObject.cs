using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    [CreateMenuTitle("Table")]
    [CreateAssetMenu(menuName = "Polyperfect/Recipes/Table")]
    public class TableRecipeObject : BaseRecipeObject, IRecipe<Serialized2DItemList, QuantityContainer>
    {
        [SerializeField] Serialized2DItemList requirements = new Serialized2DItemList(new Vector2Int(3,3));
        [SerializeField] QuantityContainer results = new QuantityContainer();
        public override string __Usage => "An object that holds table-based recipes.";
        public override IEnumerable<ItemStack> Ingredients => requirements.Select(i => new ItemStack(i.ID, i.Value));

        public override IEnumerable<ItemStack> Outputs => Output.Select(o => new ItemStack(o.ID, o.Value));

        public Serialized2DItemList Requirements => requirements;
        public QuantityContainer Output => results;
    }
}