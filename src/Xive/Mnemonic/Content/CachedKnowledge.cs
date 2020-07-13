using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xive.Mnemonic.Content
{
    /// <summary>
    /// Contents which are cached.
    /// </summary>
    public sealed class CachedKnowledge : IContents
    {
        private readonly ConcurrentBag<IList<string>> knowledgeCache;
        private readonly IContents origin;

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="byteCache"></param>
        /// <param name="xmlCache"></param>
        public CachedKnowledge(IContents origin) : this(
            origin,
            new ConcurrentBag<IList<string>>()
        )
        { }

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="byteCache"></param>
        /// <param name="xmlCache"></param>
        public CachedKnowledge(IContents origin, ConcurrentBag<IList<string>> knowledgeCache)
        {
            this.origin = origin;
            this.knowledgeCache = knowledgeCache;
        }

        public IList<string> Knowledge()
        {
            lock (this.knowledgeCache)
            {
                if (this.knowledgeCache.IsEmpty)
                {
                    this.knowledgeCache.Add(this.origin.Knowledge());
                }
                return this.knowledgeCache.ToArray()[0];
            }
        }

        public byte[] Bytes(string name, Func<byte[]> ifAbsent)
        {
            return this.Bytes(name, () =>
            {
                var bytes = ifAbsent();
                Knowledge();
                return bytes;
            });
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            return this.origin.Xml(name, () => 
            {
                InvalidateKnowledge();
                var node = ifAbsent();
                return node;
            });
        }

        public void UpdateBytes(string name, byte[] data)
        {
            name = new Normalized(name).AsString();
            lock (this.knowledgeCache)
            {
                this.origin.UpdateBytes(name, data);
                InvalidateKnowledge();
            }
        }

        public void UpdateXml(string name, XNode node)
        {
            name = new Normalized(name).AsString();
            lock (this.knowledgeCache)
            {
                this.origin.UpdateXml(name, node);
                InvalidateKnowledge();
            }
        }

        private void InvalidateKnowledge()
        {
            IList<string> unused;
            this.knowledgeCache.TryTake(out unused);
        }

        private bool IsEmpty(XNode node)
        {
            return !node.Document.Elements().GetEnumerator().MoveNext();
        }
    }
}
