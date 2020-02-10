using System.Collections.Concurrent;
using System.Threading;

namespace Xive
{
    /// <summary>
    /// A sync which synchronizes inside the process which owns the Xive.
    /// </summary>
    public sealed class SyncGate : ISyncValve
    {
        private readonly ConcurrentDictionary<string, Mutex> locks;

        /// <summary>
        /// A sync which synchronizes inside the process which owns the Xive.
        /// </summary>
        public SyncGate() : this(new ConcurrentDictionary<string, Mutex>())
        { }

        /// <summary>
        /// A sync which synchronizes inside the process which owns the Xive.
        /// </summary>
        public SyncGate(ConcurrentDictionary<string, Mutex> locks)
        {
            this.locks = locks;
        }

        public Mutex Mutex(string name)
        {
            return this.locks.GetOrAdd(new Normalized(name).AsString(), (m) => new Mutex(false));
        }
    }

}
