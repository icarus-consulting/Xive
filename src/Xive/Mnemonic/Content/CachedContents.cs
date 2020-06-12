using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xive.Mnemonic.Content
{
    public sealed class CachedContents : IContents
    {
        private readonly IContents origin;
        private readonly ConcurrentDictionary<string, byte[]> byteMem;
        private readonly ConcurrentDictionary<string, XNode> xmlMem;

        public CachedContents(IContents origin, ConcurrentDictionary<string, byte[]> byteCache, ConcurrentDictionary<string, XNode> xmlCache)
        {
            this.origin = origin;
            this.byteMem = byteCache;
            this.xmlMem = xmlCache;
        }

        public IList<string> Knowledge()
        {
            return this.origin.Knowledge();
        }

        public byte[] Bytes(string name, Func<byte[]> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var bytes = this.byteMem.GetOrAdd(name, this.origin.Bytes(name, ifAbsent));
            return bytes;
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var bytes = this.xmlMem.GetOrAdd(name, this.origin.Xml(name, ifAbsent));
            return bytes;
        }

        public void UpdateBytes(string name, byte[] data)
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

        public void UpdateXml(string name, XNode xml)
        {
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
    }
}
