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
using Xive.Cache;
using Xive.Cell;
using Xive.Xocument;

namespace Xive.Comb
{
    /// <summary>
    /// A comb which exists in memory.
    /// </summary>
    public sealed class RamComb : IHoneyComb
    {
        private readonly string name;
        private readonly IMemories memory;
        private readonly Func<IXocument, IXocument> xocumentWrap;
        private readonly Func<ICell, ICell> cellWrap;

        /// <summary>
        /// A comb which exists in memory.
        /// The contents of this comb will live as long as the comb lives.
        /// </summary>
        /// <param name="name"></param>
        public RamComb(string name) : this(name, cell => cell, xoc => xoc, new SimpleMemories())
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// The contents of this comb will live as long as the memory lives.
        /// </summary>
        public RamComb(string name, IMemories memory) : this(name, cell => cell, xoc => xoc, memory)
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// By using this ctor, the cells of this comb's ctor exist only as long as the comb instance exists.
        /// By this ctor, you can tell the comb how to wrap a Xocument.
        /// </summary>
        public RamComb(string name, Func<ICell, ICell> cellWrap) : this(name, cellWrap, xoc => xoc, new SimpleMemories())
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// By using this ctor, the cells of this comb's ctor exist only as long as the comb instance exists.
        /// By this ctor, you can tell the comb how to wrap a Xocument.
        /// </summary>
        public RamComb(string name, Func<IXocument, IXocument> xocumentWrap) : this(name, cell => cell, xocumentWrap, new SimpleMemories())
        { }

        /// <summary>
        /// A comb which exists in memory.
        /// The first Dictionary contains memory of all cells, accessible by their name.
        /// The second Dictionary contains memory of all xmls, also accessible by their name.
        /// You must tell the comb how to build a Xocument from its name and a cell.
        /// By using this ctor, every RamComb with the same name will have the same contents.
        /// </summary>
        public RamComb(string name, Func<ICell, ICell> cellWrap, Func<IXocument, IXocument> xocumentWrap, IMemories mem)
        {
            this.name = name;
            this.memory = mem;
            this.xocumentWrap = xocumentWrap;
            this.cellWrap = cellWrap;
        }

        public string Name()
        {
            return this.name;
        }

        public IProps Props()
        {
            return 
                this.memory
                    .Props()
                    .Content(
                        name, 
                        () => throw new ApplicationException($"cannot find props for '{name}' in memory. Props should have been created before the comb is created."
                    )
                );
        }

        public IXocument Xocument(string name)
        {
            return this.xocumentWrap(new RamXocument($"{this.name}/{name}", this.memory));
        }

        public ICell Cell(string name)
        {
            ICell result;
            //if (name.Equals("_guts.xml"))
            //{
            //    var itemName = new Normalized(this.name).AsString();
            //    var patch = new Directives().Add("items");
            //    new Each<string>(
            //        (key) =>
            //            patch.Add("item")
            //            .Add("name")
            //            .Set(key.Substring((itemName + "/").Length))
            //            .Up()
            //            .Add("size")
            //            .Set(this.cellMemory[key].Length)
            //            .Up()
            //            .Up(),
            //        new Filtered<string>(
            //           (path) => path.Substring(0, itemName.Length) == itemName,
            //           this.memory.Keys
            //       )
            //    ).Invoke();

            //    result =
            //            new RamCell(
            //                "_guts.xml",
            //                new MemoryStream(
            //                    new BytesOf(
            //                        new Xambler(patch).Dom().ToString()
            //                    ).AsBytes()
            //                )
            //           );
            //}
            //else
            //{
                result = 
                    this.cellWrap(
                        new RamCell($"{this.name}/{name}", this.memory)
                    );
            //}
            return result;
        }
    }
}
