using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Xive.Mnemonic.Sync
{
    /// <summary>
    /// A sync pipe which is realized using exclusive functions.
    /// </summary>
    public class LocalSyncPipe : ISyncPipe
    {
        private readonly ConcurrentDictionary<string, Action<Action>> pipe;

        /// <summary>
        /// A sync pipe which is realized using exclusive functions.
        /// </summary>
        public LocalSyncPipe()
        {
            this.pipe = new ConcurrentDictionary<string, Action<Action>>();
        }

        public void Flush(string lck, Action flush)
        {
            var exclusive = this.pipe.GetOrAdd(lck, (act) => act());
            lock (exclusive)
            {
                exclusive(flush);
            }
        }
    }
}
