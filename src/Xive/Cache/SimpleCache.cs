using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Xive.Hive
{
    /// <summary>
    /// A simple cache.
    /// </summary>
    public sealed class SimpleCache : ICache
    {
        private readonly IDictionary<string, MemoryStream> binMemory;
        private readonly IDictionary<string, XNode> xmlMemory;

        /// <summary>
        /// A simple cache.
        /// </summary>
        public SimpleCache() : this(
            new Dictionary<string, MemoryStream>(),
            new Dictionary<string, XNode>()
        )
        { }

        /// <summary>
        /// A cache built from pre-filled maps.
        /// </summary>
        /// <param name="binaries">A pre-filled cache of binaries</param>
        /// <param name="xmls">A pre-filled cache of xml nodes</param>
        public SimpleCache(
            IDictionary<string, MemoryStream> binaries,
            IDictionary<string, XNode> xmls
        )
        {
            this.binMemory = binaries;
            this.xmlMemory = xmls;
        }

        public bool Has(string name)
        {
            return this.xmlMemory.ContainsKey(name) || this.binMemory.ContainsKey(name);
        }

        public void Update(string name, MemoryStream binary)
        {
            this.binMemory[name] = binary;
        }

        public void Update(string name, XNode xNode)
        {
            this.xmlMemory[name] = xNode;
        }

        public MemoryStream Binary(string name, Func<MemoryStream> ifAbsent)
        {
            if (!binMemory.ContainsKey(name))
            {
                if (this.xmlMemory.ContainsKey(name))
                {
                    throw
                        new InvalidOperationException(
                            $"Cannot use '{name}' as Cell because it has previously been used as Xocument. "
                            + " Caching only works with consistent usage of contents."
                        );
                }
                binMemory[name] = ifAbsent();
            }
            return this.binMemory[name];
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            if (!this.xmlMemory.ContainsKey(name))
            {
                if (this.binMemory.ContainsKey(name))
                {
                    throw
                        new InvalidOperationException(
                            $"Cannot use '{name}' as Xocument because it has previously been used as Cell. "
                            + " Caching only works with consistent usage of contents."
                        );
                }
                this.xmlMemory[name] = ifAbsent();
            }
            return this.xmlMemory[name];
        }
    }
}
