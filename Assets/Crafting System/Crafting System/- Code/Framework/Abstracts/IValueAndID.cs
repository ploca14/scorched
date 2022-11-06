namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Any value associated with an ID, such as a quantity or purity
    /// </summary>
    public interface IValueAndID<T> : IValue<T>, IHasID where T : struct
    {
    }

    public static class AttributeAndIDExtensions
    {
        public static bool IsEmpty<T>(this IValueAndID<T> that) where T : struct
        {
            return that?.Value.IsDefault() ?? true;
        }
    }
}