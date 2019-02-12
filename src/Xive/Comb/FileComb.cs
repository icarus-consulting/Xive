using System;
using System.IO;
using Yaapii.Atoms.Scalar;
using Xive.Cell;
using Xive.Xocument;

namespace Xive.Comb
{
    /// <summary>
    /// A comb which exists physically as files.
    /// </summary>
    public sealed class FileComb : CombEnvelope
    {
        /// <summary>
        /// A comb which exists physically as files.
        /// </summary>
        public FileComb(string name, string root) : this(name, root, xoc => xoc)
        { }

        /// <summary>
        /// A comb which exists physically as files.
        /// </summary>
        public FileComb(string name, string root, Func<IXocument, IXocument> wrap) : base(
            new ScalarOf<IComb>(() =>
                new SimpleComb(
                    Path.Combine(root, name),
                    path => new FileCell(path),
                    (cellName, cell) => wrap(new CellXocument(cell, cellName))
                )
            )
        )
        { }
    }
}
