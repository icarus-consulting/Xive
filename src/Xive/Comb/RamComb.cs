using System.Collections.Generic;
using Xive.Cell;

namespace Xive.Comb
{
    /// <summary>
    /// A comb which exists in memory.
    /// </summary>
    public sealed class RamComb : SimpleComb
    {
        /// <summary>
        /// A comb which exists in memory.
        /// By using this ctor, the cells of this comb's ctor exist only as long as the comb instance exists. 
        /// </summary>
        public RamComb(string name) : this(name, new Dictionary<string, byte[]>())
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// The Dictionary contains memory of all cells, accessible by their name.
        /// By using this ctor, every RamComb with the same name will have the same content.
        /// </summary>
        public RamComb(string name, IDictionary<string, byte[]> cellMemory) : base(name, cell =>
            {
                return new RamCell(cell, cellMemory);
            }
        )
        { }
    }
}
