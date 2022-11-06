using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration
{
    public class TableRecipe<INGREDIENT, OUTPUT> : IRecipe<INGREDIENT, OUTPUT>
    {
        public TableRecipe(INGREDIENT requirements, OUTPUT output)
        {
            Requirements = requirements;
            Output = output;
        }

        public INGREDIENT Requirements { get; }
        public OUTPUT Output { get; }
    }
}