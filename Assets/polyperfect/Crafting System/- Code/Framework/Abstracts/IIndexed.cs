using System.Collections.Generic;

namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Something which can be accessed by a non-standard index.
    /// </summary>
    public interface IIndexed<INDEX, DATA>
    {
        DATA this[INDEX ind] { get; set; }
        IEnumerable<INDEX> Indices { get; }
    }
}