using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Xive.Hive;
using Xive.Mnemonic;

namespace Xive.Cache
{
    /// <summary>
    /// Memory for XML nodes.
    /// </summary>
    public sealed class DataRam : IMemory<MemoryStream>
    {
        private readonly ConcurrentDictionary<string, MemoryStream> mem;

        /// <summary>
        /// Memory for XML nodes.
        /// </summary>
        public DataRam() : this(new ConcurrentDictionary<string, MemoryStream>())
        { }

        /// <summary>
        /// Memory for XML nodes.
        /// </summary>
        public DataRam(ConcurrentDictionary<string, MemoryStream> mem)
        {
            this.mem = mem;
        }

        public bool Knows(string name)
        {
            return this.mem.ContainsKey(name);
        }

        public IEnumerable<string> Knowledge()
        {
            return this.mem.Keys;
        }

        public MemoryStream Content(string name, Func<MemoryStream> ifAbsent)
        {
            name = new Normalized(name).AsString();
            return this.mem.GetOrAdd(name, (key) => ifAbsent());
        }

        public void Update(string name, MemoryStream content)
        {
            name = new Normalized(name).AsString();
            if (content.Length == 0)
            {
                MemoryStream removed;
                this.mem.TryRemove(name, out removed);
            }
            else
            {
                this.mem.AddOrUpdate(name, content, (currentName, currentContent) => content);
            }
        }
    }
}
