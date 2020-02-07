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
using System.Collections.Generic;
using System.IO;
using Xive.Cache;
using Xive.Hive;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which exists in memory.
    /// </summary>
    public sealed class RamCell : ICell
    {
        private readonly IScalar<string> name;
        private readonly IScalar<IMemories> mem;
        private readonly string id;

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell() : this(
            new Solid<string>(() => Guid.NewGuid().ToString()),
            new Solid<IMemories>(() => new SimpleMemories())
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(MemoryStream content) : this(
            new ScalarOf<string>(() => Guid.NewGuid().ToString()),
            content
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells with the same path using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(string path, IBytes content) : this(
            new ScalarOf<string>(() => path),
            new Solid<IMemories>(() =>
            {
                var mem = new SimpleMemories();
                mem.Data().Update(path, new MemoryStream(content.AsBytes()));
                return mem;
            })
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells with the same path using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(IScalar<string> path, MemoryStream content) : this(
            path,
            new Solid<IMemories>(() =>
            {
                var mem = new SimpleMemories();
                mem.Data().Update(path.Value(), content);
                return mem;
            })
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells with the same path using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(string path, MemoryStream content) : this(
            new ScalarOf<string>(path),
            new Solid<IMemories>(() =>
            {
                var mem = new SimpleMemories();
                mem.Data().Update(path, content);
                return mem;
            })
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells with the same path using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(string path) : this(path, new SimpleMemories())
        { }

        /// <summary>
        /// An item which exists in memory.
        /// This cell has its internal memory stored in the itemMap. So: If you make two cells
        /// using this ctor, they will have the same memory.
        /// </summary>
        /// <param name="path">path to the item (key to the map)</param>
        /// <param name="cellMemory">map with content for many items</param>
        public RamCell(string path, IMemories mem) : this(
            new ScalarOf<string>(path),
            new ScalarOf<IMemories>(mem)
        )
        { }

        /// <summary>
        /// An item which exists in memory.
        /// This cell has its internal memory stored in the itemMap. So: If you make two cells
        /// using this ctor, they will have the same memory.
        /// </summary>
        /// <param name="path">path to the item (key to the map)</param>
        /// <param name="itemMap">map with content for many items</param>
        internal RamCell(IScalar<string> name, IScalar<IMemories> mem)
        {
            lock (mem)
            {
                this.id = Guid.NewGuid().ToString();
                this.name =
                    new Solid<string>(
                        () => new StrictCellName(name.Value()).AsString()
                    );
                this.mem = mem;
            }
        }

        public string Name()
        {
            return new Normalized(this.name.Value()).AsString();
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];
            var name = new Normalized(this.name.Value()).AsString();
            var stream = this.mem.Value().Data().Content(name, () => new MemoryStream());
            stream.Seek(0, SeekOrigin.Begin);
            result = stream.ToArray();
            return result;
        }

        public void Update(IInput content)
        {
            var name = new Normalized(this.name.Value()).AsString();
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
        {
            //Do nothing.
        }
    }
}
