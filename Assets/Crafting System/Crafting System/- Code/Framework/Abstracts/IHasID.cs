namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Guarantees the presence of an ID for lookup.
    /// </summary>
    public interface IHasID
    {
        RuntimeID ID { get; }
    }
}