using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic.Content
{
    /// <summary>
    /// Contents which are cached.
    /// </summary>
    public sealed class CachedContents : IContents
    {
        private readonly IContents origin;
        private readonly ConcurrentBag<IList<string>> knowledgeMem;
        private readonly ConcurrentDictionary<string, byte[]> byteMem;
        private readonly ConcurrentDictionary<string, XNode> xmlMem;

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="byteCache"></param>
        /// <param name="xmlCache"></param>
        public CachedContents(IContents origin) : this(
            origin,
            new ConcurrentDictionary<string, byte[]>(),
            new ConcurrentDictionary<string, XNode>()
        )
        { }

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="byteCache"></param>
        /// <param name="xmlCache"></param>
        public CachedContents(IContents origin, ConcurrentDictionary<string, byte[]> byteCache, ConcurrentDictionary<string, XNode> xmlCache)
        {
            this.knowledgeMem = new ConcurrentBag<IList<string>>();
            this.origin = origin;
            this.byteMem = byteCache;
            this.xmlMem = xmlCache;
        }

        public IList<string> Knowledge()
        {
            lock (this.knowledgeMem)
            {
                if (this.knowledgeMem.IsEmpty)
                {
                    this.knowledgeMem.Add(this.origin.Knowledge());
                }
                return this.knowledgeMem.ToArray()[0];
            }
        }

        public byte[] Bytes(string name, Func<byte[]> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var bytes = this.byteMem.GetOrAdd(name, (n) => this.origin.Bytes(name, ifAbsent));
            return bytes;
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var xml = this.xmlMem.GetOrAdd(name, (n) => this.origin.Xml(name, ifAbsent));
            return xml;
        }

        public void UpdateBytes(string name, byte[] data)
        {
            name = new Normalized(name).AsString();
            var isEmpty = data.Length == 0;
            lock (this.byteMem)
            {
                if (isEmpty)
                {
                    byte[] unused;
                    this.byteMem.TryRemove(name, out unused);
                    this.origin.UpdateBytes(name, data);
                }
                else
                {
                    this.byteMem.AddOrUpdate(
                    name,
                    (n) => data,
                    (n, existing) =>
                        {
                            this.origin.UpdateBytes(name, data);
                            return data;
                        }
                    );
                }
                InvalidateKnowledge();
            }
        }

        public void UpdateXml(string name, XNode xml)
        {
            name = new Normalized(name).AsString();
            var isEmpty = !xml.Document.Elements().GetEnumerator().MoveNext();
            lock (this.xmlMem)
            {
                if (isEmpty)
                {
                    XNode unused;
                    this.xmlMem.TryRemove(name, out unused);
                    this.origin.UpdateXml(name, xml);
                }
                else
                {
                    var elem = new FirstOf<XElement>(xml.Document.Elements()).Value();

                    this.xmlMem.AddOrUpdate(
                        name,
                        (n) => xml,
                        (n, existing) =>
                        {
                            this.origin.UpdateXml(name, xml);
                            return xml;
                        }
                    );
                }
                InvalidateKnowledge();
            }
        }

        private void InvalidateKnowledge()
        {
            IList<string> unused;
            this.knowledgeMem.TryTake(out unused);
        }
    }
}
