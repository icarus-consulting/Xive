using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;
using Xive.Mnemonic;
using Xive.Props;
using Xive.Xocument;

namespace Xive.Cache
{
    /// <summary>
    /// Simple Memories.
    /// </summary>
    public sealed class SimpleMemories : IMnemonic
    {
        private readonly IMemory<XNode> xmlMem;
        private readonly IMemory<MemoryStream> dataMem;
        private readonly ConcurrentDictionary<string, IProps> propsMem;

        /// <summary>
        /// Simple Memories.
        /// </summary>
        public SimpleMemories(IMemory<XNode> xmlMem, IMemory<MemoryStream> dataMem)
        {
            this.xmlMem = xmlMem;
            this.dataMem = dataMem;
            this.propsMem = new ConcurrentDictionary<string, IProps>();
        }

        public IProps Props(string scope, string id)
        {
            return
                this.propsMem.GetOrAdd(
                    $"{scope}/{id}",
                    key =>
                    {
                        return
                            new XocumentProps(
                                new MemorizedXocument($"{scope}/hq/catalog.xml", this),
                                scope,
                                id
                            );
                    }
                );
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
