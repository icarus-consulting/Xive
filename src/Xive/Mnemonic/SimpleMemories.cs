using System.Collections.Concurrent;
using System.Xml.Linq;
using Xive.Mnemonic;
using Xive.Props;

namespace Xive.Cache
{
    /// <summary>
    /// Simple Memories.
    /// </summary>
    public sealed class SimpleMemories : IMnemonic
    {
        private readonly IMemory<XNode> xmlMem;
        private readonly IMemory<byte[]> dataMem;
        private readonly ConcurrentDictionary<string, IProps> propsMem;

        /// <summary>
        /// Simple Memories.
        /// </summary>
        public SimpleMemories(IMemory<XNode> xmlMem, IMemory<byte[]> dataMem)
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
                        return new SandboxProps(this, scope, id);
                    }
                );
        }

        public IMemory<XNode> XML()
        {
            return this.xmlMem;
        }

        public IMemory<byte[]> Data()
        {
            return this.dataMem;
        }
    }

    /// <summary>
    /// Simple Memories.
    /// </summary>
    public sealed class SimpleMemories2 : IMnemonic2
    {
        private readonly IContents contentMem;
        private readonly ConcurrentDictionary<string, IProps> propsMem;

        /// <summary>
        /// Simple Memories.
        /// </summary>
        public SimpleMemories2(IContents contents)
        {
            this.contentMem = contents;
            this.propsMem = new ConcurrentDictionary<string, IProps>();
        }

        public IProps Props(string scope, string id)
        {
            return
                this.propsMem.GetOrAdd(
                    $"{scope}/{id}",
                    key =>
                    {
                        return new SandboxProps2(this, scope, id);
                    }
                );
        }

        public IContents Contents()
        {
            return this.contentMem;
        }
    }
}
