using System;

namespace Xive.Mnemonic
{
    public interface ISyncPipe
    {
        void Flush(string lck, Action flush);
    }
}
