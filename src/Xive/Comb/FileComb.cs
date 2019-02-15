//MIT License

//Copyright (c) 2019 ICARUS Consulting GmbH

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.IO;
using Xive.Cell;
using Xive.Xocument;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Comb
{
    /// <summary>
    /// A comb which exists physically as files.
    /// </summary>
    public sealed class FileComb : IHoneyComb
    {
        private readonly string name;
        private readonly IScalar<IHoneyComb> comb;

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
                new ScalarOf<IHoneyComb>(() =>
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
