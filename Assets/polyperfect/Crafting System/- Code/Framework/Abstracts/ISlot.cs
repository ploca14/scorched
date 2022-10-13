namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Something that can be both inserted into and extracted from.
    /// </summary>
    public interface ISlot<ARG, INOUT> : IInsert<INOUT>, IExtract<ARG, INOUT>
    {
    }
}