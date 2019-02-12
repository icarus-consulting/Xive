using System;
using System.IO;
using Xive.Cell;
using Xive.Xocument;

namespace Xive.Comb
{
    /// <summary>
    /// A comb that accesses cells systemwide exclusively.
    /// </summary>
    public sealed class SyncComb : IComb
    {
        private readonly IComb comb;

        /// <summary>
        /// A comb that accesses cells systemwide exclusively.
        /// </summary>
        public SyncComb(IComb comb)
        {
            this.comb = comb;
        }

        public string Name()
        {
            lock (comb)
            {
                return comb.Name();
            }
        }

        public ICell Cell(string name)
        {
            lock (comb)
            {
                return new SyncCell(comb.Name() + Path.DirectorySeparatorChar + name, (str) => comb.Cell(name));
            }
        }

        public IXocument Xocument(string name)
        {
            lock(comb)
            {
                return 
                    new SyncXocument(
                        name,
                        new CellXocument(Cell(name), name)
                    );
            }
        }
    }
}
