using System;
using System.Collections.Generic;
using System.Text;

namespace Xive
{
    public interface ISyncPipe
    {
        void Flush(string lck, Action flush);
    }
}
