using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xive.Hive;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memory for anything.
    /// </summary>
    public sealed class VersatileRam<T> : IMemory<T>
    {
        private readonly ConcurrentDictionary<string, T> mem;
        private readonly Func<T, bool> isEmpty;

        /// <summary>
        /// Memory for anything.
        /// </summary>
        public VersatileRam(Func<T, bool> isEmpty) : this(new ConcurrentDictionary<string, T>(), isEmpty)
        { }

        /// <summary>
        /// Memory for anything.
        /// </summary>
        public VersatileRam(ConcurrentDictionary<string, T> mem, Func<T, bool> isEmpty)
        {
            this.mem = mem;
            this.isEmpty = isEmpty;
        }

        public bool Knows(string name)
        {
            return this.mem.ContainsKey(name);
        }

        public IEnumerable<string> Knowledge()
        {
            return this.mem.Keys;
        }

        public T Content(string name, Func<T> ifAbsent)
        {
            name = new Normalized(name).AsString();
            return this.mem.GetOrAdd(name, (key) => ifAbsent());
        }

        public void Update(string name, T content)
        {
            name = new Normalized(name).AsString();
            if (this.isEmpty(content))
            {
                T removed;
                this.mem.TryRemove(name, out removed);
            }
            else
            {
                this.mem.AddOrUpdate(name, content, (currentName, currentContent) => content);
            }
        }
    }
}
