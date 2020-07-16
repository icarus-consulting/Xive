using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Mnemonic.Cache;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic.Content
{
    /// <summary>
    /// Contents which are cached.
    /// </summary>
    public sealed class CachedContents : IContents
    {
        private readonly IContents origin;
        private readonly ICache<byte[]> byteCache;
        private readonly ICache<XNode> xmlCache;

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        public CachedContents(IContents origin) : this(
            origin,
            new BytesCache(),
            new XmlCache()
        )
        { }

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        public CachedContents(IContents origin, IEnumerable<string> ignored, long maxSize) : this(
            origin,
            new IgnoringCache<byte[]>(
                new BytesCache(maxSize),
                ignored
            ),
            new IgnoringCache<XNode>(
                new XmlCache(),
                ignored
            )
        )
        { }

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="byteCache"></param>
        /// <param name="xmlCache"></param>
        public CachedContents(IContents origin, ICache<byte[]> byteCache, ICache<XNode> xmlCache)
        {
            this.origin = origin;
            this.byteCache = byteCache;
            this.xmlCache = xmlCache;
        }

        public IList<string> Knowledge(string filter = "")
        {
            return this.origin.Knowledge(filter);
        }

        public byte[] Bytes(string name, Func<byte[]> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var bytes = this.byteCache.Content(name, () => this.origin.Bytes(name, ifAbsent));
            return bytes;
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var xml = this.xmlCache.Content(name, () => this.origin.Xml(name, ifAbsent));
            return xml;
        }

        public void UpdateBytes(string name, byte[] data)
        {
            name = new Normalized(name).AsString();
            var isEmpty = data.Length == 0;
            lock (this.byteCache)
            {
                if (isEmpty)
                {
                    this.byteCache.Remove(name);
                }
                else
                {
                    this.byteCache
                        .Update(
                            name, 
                            () => data,
                            () => data
                        );
                }
                this.origin.UpdateBytes(name, data);
            }
        }

        public void UpdateXml(string name, XNode xml)
        {
            name = new Normalized(name).AsString();
            var isEmpty = !xml.Document.Elements().GetEnumerator().MoveNext();
            lock (this.xmlCache)
            {
                if (isEmpty)
                {
                    this.xmlCache.Remove(name);
                }
                else
                {
                    this.xmlCache
                        .Update(
                            name,
                            () => xml,
                            () => xml
                        );
                }
                this.origin.UpdateXml(name, xml);
            }
        }
    }
}
