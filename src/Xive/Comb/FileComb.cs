using System;
using System.IO;
using Yaapii.Atoms.Scalar;
using Xive.Cell;
using Xive.Xocument;
using Yaapii.Atoms;

namespace Xive.Comb
{
    /// <summary>
    /// A comb which exists physically as files.
    /// </summary>
    public sealed class FileComb : IComb
    {
        private readonly string name;
        private readonly IScalar<IComb> comb;

        /// <summary>
        /// A comb which exists physically as files.
        /// </summary>
        public FileComb(string root, string name) : this(root, name, cell => cell, xoc => xoc)
        { }

        /// <summary>
        /// A comb which exists physically as files.
        /// </summary>
        public FileComb(string root, string name, Func<ICell, ICell> cellWrap, Func<IXocument, IXocument> xocumentWrap)
        {
            this.comb =
                new ScalarOf<IComb>(() =>
                    new SimpleComb(
                        Path.Combine(root, name),
                        path => new FileCell(path),
                        (cellName, cell) => xocumentWrap(new CellXocument(cell, cellName))
                    )
                );
            this.name = name;

        }

        public ICell Cell(string name)
        {
            return this.comb.Value().Cell(name);
        }

        public string Name()
        {
            return this.name;
        }

        public IXocument Xocument(string name)
        {
            return this.comb.Value().Xocument(name);
        }
    }
}
