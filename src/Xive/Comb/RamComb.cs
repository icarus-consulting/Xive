using System;
using System.Collections.Generic;
using System.Text;
using Xive.Cache;
using Xive.Mnemonic;

namespace Xive.Comb
{
    /// <summary>
    /// A comb that lives in memory.
    /// </summary>
    public sealed class RamComb : IHoneyComb
    {
        private readonly IHoneyComb core;

        /// <summary>
        /// A comb that lives in memory.
        /// </summary>
        public RamComb(string name) : this(name, new RamMemories())
        { }

        /// <summary>
        /// A comb that lives in memory.
        /// </summary>
        internal RamComb(string name, IMemories mem)
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
