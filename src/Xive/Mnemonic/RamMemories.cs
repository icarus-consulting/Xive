using System.IO;
using System.Xml.Linq;
using Xive.Cache;
using Xive.Hive;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memories in Ram.
    /// </summary>
    public sealed class RamMemories : IMemories
    {
        private readonly IMemories mem;

        /// <summary>
        /// Memories in Ram.
        /// </summary>
        public RamMemories()
        {
            this.mem = new SimpleMemories(new XmlRam(), new DataRam());
        }

        public IMemory<MemoryStream> Data()
        {
            return this.mem.Data();
        }

        public IProps Props(string scope, string id)
        {
            return this.mem.Props(scope, id);
        }

        public IMemory<XNode> XML()
        {
            return this.mem.XML();
        }
    }
}
