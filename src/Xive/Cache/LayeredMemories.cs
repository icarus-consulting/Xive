using System.IO;
using System.Xml.Linq;
using Xive.Hive;

namespace Xive.Cache
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

        public IMemory<IProps> Props()
        {
            return new LayeredMemory<IProps>(this.layer, this.origin.Props());
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
