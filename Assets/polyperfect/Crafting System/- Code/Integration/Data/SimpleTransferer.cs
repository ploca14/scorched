using System;
using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration
{
    public class SimpleTransferer<T> : ITransfer
    {
        readonly IInsert<T> _dest;
        readonly IExtract<T> _source;

        public SimpleTransferer(IExtract<T> source, IInsert<T> dest)
        {
            _source = source;
            _dest = dest;
        }

        public bool CanTransfer()
        {
            return _source.CanExtract() && _dest.CanInsertCompletely(_source.Peek());
        }

        public void Transfer()
        {
            if (!CanTransfer())
                throw new Exception("Failure on transferring. Check beforehand with CanTransfer");
            _dest.InsertCompletely(_source.ExtractAll());
        }
    }
}