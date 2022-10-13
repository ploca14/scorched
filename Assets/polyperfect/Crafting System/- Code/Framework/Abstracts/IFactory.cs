namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Simple factory.
    /// </summary>
    public interface IFactory<out T>
    {
        T Create();
    }

    /// <summary>
    ///     Produces based on an input.
    /// </summary>
    public interface IFactory<in I, out O>
    {
        O Create(I input);
    }
}