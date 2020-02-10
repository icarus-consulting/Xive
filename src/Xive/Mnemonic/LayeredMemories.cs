using System.IO;
using System.Xml.Linq;
using Xive.Cache;
using Xive.Hive;

namespace Xive.Mnemonic
{
    public sealed class LayeredMemories : IMemories
    {
        private readonly string layer;
        private readonly IMemories origin;

        public LayeredMemories(string layer, IMemories origin)
        {
            this.layer = layer;
            this.origin = origin;
        }

        public IProps Props(string scope, string id)
        {
            return this.origin.Props(scope, id);
        }

        public IMemory<XNode> XML()
        {
            return new LayeredMemory<XNode>(this.layer, this.origin.XML());
        }

        public IMemory<MemoryStream> Data()
        {
            return new LayeredMemory<MemoryStream>(this.layer, this.origin.Data());
        }
    }
}
