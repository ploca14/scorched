namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Contains a selection of Ingredients necessary for construction.
    /// </summary>
    public interface IRecipe<out INGREDIENT, out OUTPUT>
    {
        INGREDIENT Requirements { get; }
        OUTPUT Output { get; }
    }
}