using UnityEngine;

namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Anything that can have something extracted from it.
    /// </summary>
    /// <typeparam name="OUTPUT">That which can be extracted.</typeparam>
    public interface IExtract<out OUTPUT> : IPeek<OUTPUT>
    {
        OUTPUT ExtractAll();
        bool CanExtract();
    }

    /// <summary>
    ///     Anything which can have something extracted from it using some type of argument.
    /// </summary>
    /// <typeparam name="OUTPUT">That which can be extracted.</typeparam>
    /// <typeparam name="ARG">The argument that affects what is extracted.</typeparam>
    public interface IExtract<in ARG, out OUTPUT> : IPeek<ARG, OUTPUT>, IExtract<OUTPUT>
    {
        /// <summary>
        ///     Extracts as much as possible given the argument.
        /// </summary>
        OUTPUT ExtractAmount(ARG arg);
    }
}