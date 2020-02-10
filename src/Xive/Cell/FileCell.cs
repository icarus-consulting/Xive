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

using System.IO;
using Xive.Cache;
using Xive.Mnemonic;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which exists physically as a file.
    /// </summary>
    public sealed class FileCell : ICell
    {
        private readonly Sticky<IMemories> mem;
        private readonly string name;
        private readonly string root;

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        public FileCell(string name) : this(name, new Sticky<IMemories>(() =>
            {
                var root = Path.GetDirectoryName(name);
                return new FileMemories(root);
            })
        )
        { }

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        private FileCell(string name, Sticky<IMemories> mem)
        {
            this.mem = mem;
            this.name = name;
        }

        public string Name()
        {
            return this.name;
        }

        public byte[] Content()
        {
            return
                this.mem
                    .Value()
                    .Data()
                    .Content(this.name, () => new MemoryStream()).ToArray();
        }

        public void Update(IInput content)
        {
            var name = new Normalized(Path.Combine(root, this.name)).AsString();
            var stream = content.Stream();
            if (stream.Length > 0)
            {
                var memory = new MemoryStream();
                content.Stream().CopyTo(memory);
                memory.Seek(0, SeekOrigin.Begin);
                content.Stream().Seek(0, SeekOrigin.Begin);
                this.mem.Value().Data().Update(name, memory);
            }
            else
            {
                this.mem.Value().Data().Update(name, new MemoryStream());
            }
        }

        public void Dispose()
        { }
    }
}
