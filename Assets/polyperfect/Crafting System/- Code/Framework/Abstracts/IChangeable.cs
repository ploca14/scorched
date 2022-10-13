namespace Polyperfect.Crafting.Framework
{
    public delegate void PolyChangeEvent();

    
    /// <summary>
    ///     Anything that can change and send a generic event saying so.
    /// </summary>
    public interface IChangeable
    {
        event PolyChangeEvent Changed;
    }
}