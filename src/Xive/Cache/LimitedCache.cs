using System;
using System.IO;
using System.Xml.Linq;

namespace Xive.Hive
{
    /// <summary>
    /// A cache that does only cache when items are smaller than the given bytes size.
    /// </summary>
    public sealed class LimitedCache : ICache
    {
        private readonly int maxBytes;
        private readonly ICache origin;

        /// <summary>
        /// A cache that does only cache when items are smaller than the given bytes size.
        /// </summary>
        public LimitedCache(int maxBytes, ICache origin)
        {
            this.maxBytes = maxBytes;
            this.origin = origin;
        }

        public MemoryStream Binary(string name, Func<MemoryStream> ifAbsent)
        {
            MemoryStream result;
            if (!this.origin.Has(name))
            {
                result = ifAbsent();
                if (result.Length <= this.maxBytes)
                {
                    this.origin.Binary(name, () => result);
                }
            }
            else
            {
                result = this.origin.Binary(name, () => new MemoryStream());
            }
            return result;
        }

        public void Remove(string name)
        {
            this.origin.Remove(name);
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            return this.origin.Xml(name, ifAbsent);
        }

        public bool Has(string name)
        {
            return this.origin.Has(name);
        }
    }
}
