using System;
using System.IO;
using System.Xml.Linq;
using Xive.Cache;
using Xive.Hive;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic
{
    public sealed class CachedMemories : IMemories
    {
        private readonly IMemories origin;
        private readonly Sticky<IMemory<MemoryStream>> data;
        private readonly Sticky<IMemory<XNode>> xml;

        public CachedMemories(IMemories origin)
        {
            this.origin = origin;
            this.data =
                new Sticky<IMemory<MemoryStream>>(() =>
                    new CachedMemory<MemoryStream>(origin.Data())
                );
            this.xml =
                new Sticky<IMemory<XNode>>(() =>
                    new CachedMemory<XNode>(origin.XML())
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
