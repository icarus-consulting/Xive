using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic.Content
{
    /// <summary>
    /// Contents which are cached.
    /// </summary>
    public sealed class CachedXmlContents : IContents
    {
        private readonly IContents origin;
        private readonly ConcurrentDictionary<string, XNode> xmlMem;
        private readonly IList<string> blacklist;

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="byteCache"></param>
        /// <param name="xmlCache"></param>
        public CachedXmlContents(IContents origin) : this(
            origin,
            new ConcurrentDictionary<string, XNode>(),
            new ManyOf()
        )
        { }

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="byteCache"></param>
        /// <param name="xmlCache"></param>
        public CachedXmlContents(IContents origin, ConcurrentDictionary<string, XNode> xmlCache, IEnumerable<string> blacklist)
        {
            this.origin = origin;
            this.xmlMem = xmlCache;
            this.blacklist =
                new ListOf<string>(
                    new ManyOf(() =>
                    {
                        var blacks = new List<string>(blacklist);
                        var compiled = new string[blacks.Count];
                        for (int i = 0; i < blacks.Count; i++)
                        {
                            compiled[i] = Regex.Escape(blacks[i].ToLower()).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                        }
                        return compiled;
                    })
                );
        }

        public IList<string> Knowledge(string filter = "")
        {
            return this.origin.Knowledge(filter);
        }

        public byte[] Bytes(string name, Func<byte[]> ifAbsent)
        {
            return this.origin.Bytes(name, ifAbsent);
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            name = new Normalized(name).AsString();
            XNode result;
            if (!this.xmlMem.ContainsKey(name))
            {
                result = this.origin.Xml(name, ifAbsent);
                if (!IsBlacklisted(name))
                {
                    this.xmlMem.AddOrUpdate(
                        name, (n) => result, (n, current) => result
                    );
                }
            }
            else
            {
                this.xmlMem.TryGetValue(
                    name, out result
                );
            }
            var xml = this.xmlMem.GetOrAdd(name, (n) => this.origin.Xml(name, ifAbsent));
            return xml;
        }

        public void UpdateBytes(string name, byte[] data)
        {
            name = new Normalized(name).AsString();
            var isEmpty = data.Length == 0;
            lock (this.origin)
            {
                this.origin.UpdateBytes(name, data);
                InvalidateCache(name);
            }
        }

        public void UpdateXml(string name, XNode xml)
        {
            name = new Normalized(name).AsString();
            var isEmpty = !xml.Document.Elements().GetEnumerator().MoveNext();

            lock (this.xmlMem)
            {
                if (isEmpty || IsBlacklisted(name))
                {
                    InvalidateCache(name);
                    this.origin.UpdateXml(name, xml);
                }
                else
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

        private void InvalidateCache(string name)
        {
            XNode unused;
            this.xmlMem.TryRemove(new Normalized(name).AsString(), out unused);
        }

        private bool IsBlacklisted(string name)
        {
            var result = false;
            for (var i = 0; i < this.blacklist.Count; i++)
            {
                if (Regex.IsMatch(name, this.blacklist[i]))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
