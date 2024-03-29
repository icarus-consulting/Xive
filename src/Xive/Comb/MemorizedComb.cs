﻿//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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

using System.IO;
using Xive.Cell;
using Xive.Mnemonic;
using Xive.Xocument;
using Yaapii.Atoms;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Func;
using Yaapii.Atoms.Scalar;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Comb
{
    /// <summary>
    /// A simple comb.
    /// </summary>
    public sealed class MemorizedComb : IHoneyComb
    {
        private readonly IText name;
        private readonly IMnemonic memory;

        /// <summary>
        /// A comb which exists in memory.
        /// By using this ctor, every RamComb with the same name will have the same contents.
        /// </summary>
        public MemorizedComb(string name, IMnemonic mem)
        {
            this.name = new Normalized(name);
            this.memory = mem;
        }

        public string Name()
        {
            return this.name.AsString();
        }

        public IProps Props()
        {
            var id = this.name.AsString().Substring(this.name.AsString().LastIndexOf("/") + 1);
            var root = id;
            if (this.name.AsString().Contains("/"))
            {
                root = this.name.AsString().Substring(0, this.name.AsString().LastIndexOf("/"));
            }
            return
                this.memory
                    .Props(root, id);
        }

        public IXocument Xocument(string name)
        {
            IXocument result;
            if (name.Equals("_guts.xml"))
            {
                Directives patch = GutsDirectives();
                result = new ReadOnlyXocument(
                    new XMLCursor(new Xambler(patch).Dom())
                );
            }
            else
            {
                result = new MemorizedXocument($"{this.name.AsString()}/{name}", this.memory);
            }
            return result;
        }

        public ICell Cell(string name)
        {
            ICell result;
            if (name.Equals("_guts.xml"))
            {
                var patch = GutsDirectives();
                result =
                        new RamCell(
                            "_guts.xml",
                            new MemoryStream(
                                new BytesOf(
                                    new Xambler(patch).Dom().ToString()
                                ).AsBytes()
                            )
                       );
            }
            else
            {
                result = new MemorizedCell($"{this.name}/{name}", this.memory);
            }
            return result;
        }

        private Directives GutsDirectives()
        {
            var patch = new Directives().Add("items");
            var knowledge = this.memory.Contents().Knowledge(this.name.AsString());
            new Each<string>((key) =>
                patch
                    .Add("item")
                    .Add("name")
                    .Set(key.Substring((this.name.AsString() + "/").Length))
                    .Up()
                    .Add("size")
                    .Set(
                        new LengthOf(
                            this.memory
                            .Contents()
                            .Bytes(key, () => new byte[0])
                        ).Value()
                    )
                    .Up()
                    .Up(),
            knowledge
            ).Invoke();
            return patch;
        }
    }
}
