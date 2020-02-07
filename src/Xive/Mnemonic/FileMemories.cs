using System.IO;
using System.Xml.Linq;
using Xive.Cache;
using Xive.Hive;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memories in Files.
    /// </summary>
    public sealed class FileMemories : IMemories
    {
        private readonly SimpleMemories mem;

        /// <summary>
        /// Memories in Files.
        /// </summary>
        public FileMemories(string root)
        {
            this.mem =
                new SimpleMemories(
                    new XmlInFiles(root),
                    new DataInFiles(root)
                );
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
