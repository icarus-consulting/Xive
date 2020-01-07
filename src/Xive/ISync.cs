using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Xive
{
    public interface ISyncValve
    {
        Mutex Mutex(string name);
    }
}
