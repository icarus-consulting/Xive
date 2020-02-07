using System.IO;
using System.Xml.Linq;
using Xive.Hive;

namespace Xive.Cache
{
    public sealed class SimpleMemories : IMemories
    {
        private readonly IMemory<XNode> xmlMem;
        private readonly IMemory<MemoryStream> dataMem;
        private readonly IMemory<IProps> propsMem;

        public SimpleMemories() : this(new XmlRam(), new DataRam(), new PropsRam())
        { }

        public SimpleMemories(IMemory<XNode> xmlMem, IMemory<MemoryStream> dataMem, IMemory<IProps> propsMem)
        {
            this.xmlMem = xmlMem;
            this.dataMem = dataMem;
            this.propsMem = propsMem;
        }

        public IMemory<IProps> Props()
        {
            return this.propsMem;
        }

        public IMemory<XNode> XML()
        {
            return this.xmlMem;
        }

        public IMemory<MemoryStream> Data()
        {
            return this.dataMem;
        }
    }
}
