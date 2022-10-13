namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Can have its value looked at without alteration.
    /// </summary>
    public interface IPeek<out OUTPUT>
    {
        OUTPUT Peek();
    }

    /// <summary>
    ///     Can have a potential output value based on some input queried, without altering the original.
    /// </summary>
    public interface IPeek<in ARG, out OUTPUT>
    {
        OUTPUT Peek(ARG arg);
    }
}