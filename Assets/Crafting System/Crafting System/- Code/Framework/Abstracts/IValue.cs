namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Any particular main value associated with something.
    /// </summary>
    public interface IValue<T> : IReadOnlyAttribute<T> where T : struct
    {
        new T Value { get; set; }
    }

    /// <summary>
    ///     Any particular main value associated with something.
    /// </summary>
    public interface IReadOnlyAttribute<out T> where T : struct
    {
        T Value { get; }
    }
}