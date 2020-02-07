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

        public IProps Props(string name, Func<IProps> ifAbsent)
        {
            return this.origin.Props(name, ifAbsent);
        }

        public MemoryStream Binary(string name, Func<MemoryStream> ifAbsent)
        {
            var exists = true;
            this.origin.Binary(name, () => { exists = false; return new MemoryStream(); });
            MemoryStream result;
            if (!exists)
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

        public void Update(string name, MemoryStream binary)
        {
            if (binary.Length <= this.maxBytes)
            {
                this.origin.Update(name, binary);
            }
            else
            {
                this.origin.Update(name, new MemoryStream());
            }
        }

        public void Update(string name, XNode xNode)
        {
            this.origin.Update(name, xNode);
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            return this.origin.Xml(name, ifAbsent);
        }
    }
}
