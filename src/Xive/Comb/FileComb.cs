using Xive.Cache;
using Xive.Mnemonic;

namespace Xive.Comb
{
    /// <summary>
    /// A comb that is stored in a file.
    /// </summary>
    public sealed class FileComb : IHoneyComb
    {
        private readonly MemorizedComb core;

        /// <summary>
        /// A comb that is stored in a file.
        /// </summary>
        public FileComb(string root, string name) : this(name, new FileMemories(root))
        { }

        /// <summary>
        /// A comb that is stored in a file.
        /// </summary>
        internal FileComb(string name, IMemories mem)
        {
            this.core = new MemorizedComb(name, mem);
        }

        public ICell Cell(string name)
        {
            return this.core.Cell(name);
        }

        public string Name()
        {
            return this.core.Name();
        }

        public IProps Props()
        {
            return this.core.Props();
        }

        public IXocument Xocument(string name)
        {
            return this.core.Xocument(name);
        }
    }
}
