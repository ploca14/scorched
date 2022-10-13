using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;

namespace Polyperfect.Common.Edit
{
    /// <summary>
    ///     Easy lookup of items from Object sources
    /// </summary>
    public static class EditorItemDatabase
    {
        static EditorItemDatabase()
        {
            var dataLookup = new SpecifiedDataAccessorLookup<RuntimeID>();

            dataLookup.SetAccessor(() => new EditorItemNameAccessor());
            dataLookup.SetAccessor(() => new EditorItemIconAccessor());

            Data = dataLookup;
        }

        public static IReadOnlyDataAccessorLookup<RuntimeID> Data { get; }
    }
}