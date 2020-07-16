using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.List;

namespace Xive.Mnemonic.Content
{
    /// <summary>
    /// Contents which are cached.
    /// </summary>
    public sealed class CachedByteContents : IContents
    {
        private readonly IContents origin;
        private readonly ConcurrentDictionary<string, byte[]> byteMem;
        private readonly int maxCachedSize;
        private readonly IList<string> blacklist;

        /// <summary>
        /// Contents which are cached.
        /// XNodes are cached fully parsed.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="byteCache"></param>
        /// <param name="xmlCache"></param>
        public CachedByteContents(IContents origin, int maxCachedSize) : this(
            origin,
            new ConcurrentDictionary<string, byte[]>(),
            maxCachedSize,
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
        public CachedByteContents(IContents origin, ConcurrentDictionary<string, byte[]> byteCache, int maxCachedSize, IEnumerable<string> blacklist)
        {
            this.origin = origin;
            this.byteMem = byteCache;
            this.maxCachedSize = maxCachedSize;
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
            name = new Normalized(name).AsString();
            byte[] result;
            if (!this.byteMem.ContainsKey(name))
            {
                result = this.origin.Bytes(name, ifAbsent);
                if (result.Length <= this.maxCachedSize && !IsBlacklisted(name))
                {
                    this.byteMem.AddOrUpdate(
                        name, (n) => result, (n, current) => result
                    );
                }
            }
            else
            {
                this.byteMem.TryGetValue(
                    name, out result
                );
            }
            return result;
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            return this.origin.Xml(name, ifAbsent);
        }

        public void UpdateBytes(string name, byte[] data)
        {
            name = new Normalized(name).AsString();
            var isEmpty = data.Length == 0;
            var shouldCache = data.Length <= this.maxCachedSize;
            lock (this.byteMem)
            {
                if (isEmpty || !shouldCache)
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
            }
        }

        public void UpdateXml(string name, XNode xml)
        {
            lock (this.byteMem)
            {
                this.origin.UpdateXml(name, xml);
            }
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
