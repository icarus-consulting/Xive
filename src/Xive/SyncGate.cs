using System.Collections.Concurrent;
using System.Threading;

namespace Xive
{
    /// <summary>
    /// A sync which synchronizes inside the process which owns the Xive.
    /// </summary>
    public sealed class ProcessSyncValve : ISyncValve
    {
        private readonly ConcurrentDictionary<string, Mutex> locks;

        /// <summary>
        /// A sync which synchronizes inside the process which owns the Xive.
        /// </summary>
        public ProcessSyncValve()
        {
            this.locks = new ConcurrentDictionary<string, Mutex>();
        }

        public Mutex Mutex(string name)
        {
            return this.locks.GetOrAdd(new Normalized(name).AsString(), (m) => new Mutex());
        }
    }

}
