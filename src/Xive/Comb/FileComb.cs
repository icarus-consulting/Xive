using System;
using System.IO;
using Yaapii.Atoms.Scalar;
using Xive.Cell;

namespace Xive.Comb
{
    /// <summary>
    /// A comb which exists physically as files.
    /// </summary>
    public sealed class FileComb : SimpleComb
    {
        /// <summary>
        /// A comb which exists physically as files.
        /// </summary>
        public FileComb(string name, string root) : this(name, root, (path) => new FileCell(path))
        { }

        /// <summary>
        /// A comb which exists physically as files.
        /// </summary>
        public FileComb(string name, string root, Func<string, ICell> cell) : base(
            root,
            name, 
            cell)
        { }
    }
}
