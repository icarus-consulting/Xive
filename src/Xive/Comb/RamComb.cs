using System;
using System.Collections.Generic;
using System.IO;
using Xive.Cell;
using Xive.Xocument;

namespace Xive.Comb
{
    /// <summary>
    /// A comb which exists in memory.
    /// </summary>
    public sealed class RamComb : IComb
    {
        private readonly string name;
        private readonly IDictionary<string, byte[]> cellMemory;
        private readonly Func<IXocument, IXocument> xocumentWrap;
        private readonly Func<ICell, ICell> cellWrap;

        /// <summary>
        /// A comb which exists in memory.
        /// The contents of this comb will live as long as the comb lives.
        /// </summary>
        /// <param name="name"></param>
        public RamComb(string name) : this(name, cell => cell, xoc => xoc, new Dictionary<string, byte[]>())
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// The contents of this comb will live as long as the memory lives.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cellmemory"></param>
        /// <param name="xmlMemory"></param>
        public RamComb(string name, IDictionary<string, byte[]> cellmemory) : this(name, cell => cell, xoc => xoc, cellmemory)
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// By using this ctor, the cells of this comb's ctor exist only as long as the comb instance exists.
        /// By this ctor, you can tell the comb how to wrap a Xocument.
        /// </summary>
        public RamComb(string name, Func<ICell, ICell> cellWrap) : this(name, cellWrap, xoc => xoc, new Dictionary<string, byte[]>())
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// By using this ctor, the cells of this comb's ctor exist only as long as the comb instance exists.
        /// By this ctor, you can tell the comb how to wrap a Xocument.
        /// </summary>
        public RamComb(string name, Func<IXocument, IXocument> xocumentWrap) : this(name, cell => cell, xocumentWrap, new Dictionary<string, byte[]>())
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// The first Dictionary contains memory of all cells, accessible by their name.
        /// The second Dictionary contains memory of all xmls, also accessible by their name.
        /// You must tell the comb how to build a Xocument from its name and a cell.
        /// By using this ctor, every RamComb with the same name will have the same contents.
        /// </summary>
        public RamComb(string name, Func<ICell, ICell> cellWrap, Func<IXocument, IXocument> xocumentWrap, IDictionary<string, byte[]> cellMemory)
        {
            this.name = name;
            this.cellMemory = cellMemory;
            this.xocumentWrap = xocumentWrap;
            this.cellWrap = cellWrap;

        }

        public string Name()
        {
            return this.name;
        }

        public IXocument Xocument(string name)
        {
            return this.xocumentWrap(new CellXocument(Cell(name), name));
        }

        public ICell Cell(string name)
        {
            return this.cellWrap(new RamCell($"{this.name}{Path.DirectorySeparatorChar}{name}", cellMemory));
        }
    }
}
