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
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Comb
{
    /// <summary>
    /// A simple comb that is dumb: You need to tell it,
    /// how to create items.
    /// </summary>
    public class SimpleComb : IHoneyComb
    {
        private readonly IScalar<string> name;
        private readonly Func<string, ICell> cell;
        private readonly Func<string, ICell, IXocument> xocument;

        /// <summary>
        /// A simple comb that is dumb: You need to tell it,
        /// how to create cells and how to create a xocument.
        /// </summary>
        public SimpleComb(string name, Func<string, ICell> cell, Func<string, ICell, IXocument> xocument) : this(new ScalarOf<string>(name), cell, xocument)
        { }

        /// <summary>
        /// A simple comb that is dumb: You need to tell it,
        /// how to create cells and how to create a xocument.
        /// </summary>
        public SimpleComb(IScalar<string> name, Func<string, ICell> cell, Func<string, ICell, IXocument> xocument)
        {
            this.name = name;
            this.cell = cell;
            this.xocument = xocument;
        }

        //public IProps Props()
        //{
        //}

        public ICell Cell(string name)
        {
            return 
                this.cell($"{this.name.Value()}/{name}");
        }

        public string Name()
        {
            return this.name.Value();
        }

        public IXocument Xocument(string name)
        {
            return this.xocument(name, this.Cell(name));
        }
    }
}
