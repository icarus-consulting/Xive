using System;

namespace Xive
{
    public interface ISyncPipe
    {
        void Flush(string lck, Action flush);
    }
}
