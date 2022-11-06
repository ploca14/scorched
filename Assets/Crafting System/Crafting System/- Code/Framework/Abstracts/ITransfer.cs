namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Transfers something from one place to another.
    /// </summary>
    public interface ITransfer
    {
        bool CanTransfer();
        void Transfer();
    }

    /// <summary>
    ///     Transfers something from one place to another based on some argument.
    /// </summary>
    public interface ITransfer<in ARG, out TRANSFERED>
    {
        bool CanTransfer(ARG arg);
        
        /// <returns>Transferred</returns>
        TRANSFERED TransferPossible(ARG arg);
    }
}