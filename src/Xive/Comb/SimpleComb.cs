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

using Xive.Cache;
using Xive.Cell;
using Xive.Xocument;
using Yaapii.Atoms;

namespace Xive.Comb
{
    /// <summary>
    /// A simple comb.
    /// </summary>
    public sealed class SimpleComb : IHoneyComb
    {
        private readonly IText name;
        private readonly IMemories memory;

        /// <summary>
        /// A comb which exists in memory.
        /// By using this ctor, every RamComb with the same name will have the same contents.
        /// </summary>
        public SimpleComb(string name, IMemories mem)
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
            var root = string.Empty;
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
            return new MemorizedXocument($"{this.name.AsString()}/{name}", this.memory);
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
            result = new RamCell($"{this.name}/{name}", this.memory);
            //}
            return result;
        }
    }
}
