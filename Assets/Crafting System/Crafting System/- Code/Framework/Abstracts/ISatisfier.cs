namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Rates the satisfaction of a REQUIREMENT with a SUPPLIED
    /// </summary>
    public interface ISatisfier<in REQUIREMENT, in SUPPLIED, out OUTPUT>
    {
        OUTPUT SatisfactionWith(REQUIREMENT requirements, SUPPLIED supplied);
    }

    /// <summary>
    ///     Rates the satisfaction of a REQUIREMENT with a SUPPLIED, where REQUIREMENT and SUPPLIED are the same type for
    ///     convenience
    /// </summary>
    public interface ISatisfier<in REQUIREDANDSUPPLIED, out OUTPUT> : ISatisfier<REQUIREDANDSUPPLIED, REQUIREDANDSUPPLIED, OUTPUT>
    {
    }
}