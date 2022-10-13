namespace Polyperfect.Crafting.Framework
{
    public interface IMultiplicable<in I, out O>
    {
        O Multiply(I input);
    }
}