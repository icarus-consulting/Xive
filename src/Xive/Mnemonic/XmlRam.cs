using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Hive;
using Xive.Mnemonic;

namespace Xive.Cache
{
    /// <summary>
    /// Memory for XML nodes.
    /// </summary>
    public sealed class XmlRam : IMemory<XNode>
    {
        private readonly ConcurrentDictionary<string, XNode> mem;

        /// <summary>
        /// Memory for XML nodes.
        /// </summary>
        public XmlRam() : this(new ConcurrentDictionary<string, XNode>())
        { }

        /// <summary>
        /// Memory for XML nodes.
        /// </summary>
        public XmlRam(ConcurrentDictionary<string, XNode> mem)
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

        public XNode Content(string name, Func<XNode> ifAbsent)
        {
            name = new Normalized(name).AsString();
            return this.mem.GetOrAdd(name, (key) => ifAbsent());
        }

        public void Update(string name, XNode content)
        {
            name = new Normalized(name).AsString();
            this.mem.AddOrUpdate(name, content, (currentName, currentContent) => content);
        }
    }
}
