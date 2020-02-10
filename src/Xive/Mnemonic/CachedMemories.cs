using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Xive.Cache;
using Xive.Hive;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memories which a stored in a cache.
    /// Update is transferred to the origin memories.
    /// </summary>
    public sealed class CachedMemories : IMemories
    {
        private readonly IMemories origin;
        private readonly Sticky<IMemory<MemoryStream>> data;
        private readonly Sticky<IMemory<XNode>> xml;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="maxSize"></param>
        public CachedMemories(IMemories origin, params string[] items) : this(origin, int.MaxValue, new List<string>(new EnumerableOf<string>(items)))
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="maxSize"></param>
        public CachedMemories(IMemories origin) : this(origin, int.MaxValue, new List<string>())
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="maxSize"></param>
        public CachedMemories(IMemories origin, int maxSize) : this(origin, maxSize, new List<string>())
        { }

        /// <summary>
        /// Memories which a stored in a cache.
        /// Update is transferred to the origin memories.
        /// </summary>
        public CachedMemories(IMemories origin, int maxSize, IEnumerable<string> blacklist) : this(
            origin,
            maxSize,
            new ListOf<string>(
                new StickyEnumerable<string>(
                    new Yaapii.Atoms.Enumerable.Mapped<string, string>(
                        entry => new Normalized(entry).AsString(),
                        blacklist
                    )
                )
            )
        )
        { }

        /// <summary>
        /// Memories which a stored in a cache.
        /// Update is transferred to the origin memories.
        /// </summary>
        internal CachedMemories(IMemories origin, int maxSize, IList<string> blacklist)
        {
            this.origin = origin;
            this.data =
                new Sticky<IMemory<MemoryStream>>(() =>
                    new CachedMemory<MemoryStream>(
                        origin.Data(),
                        maxSize,
                        stream => stream.Length == 0,
                        blacklist
                    )
                );
            this.xml =
                new Sticky<IMemory<XNode>>(() =>
                    new CachedMemory<XNode>(
                        origin.XML(),
                        maxSize,
                        node => node.Document.Root.HasElements,
                        blacklist
                    )
                );
        }

        public IMemory<MemoryStream> Data()
        {
            return this.data.Value();
        }

        public IProps Props(string scope, string id)
        {
            return this.origin.Props(scope, id); //props are already cached by design.
        }

        public IMemory<XNode> XML()
        {
            return this.xml.Value();
        }
    }
}
