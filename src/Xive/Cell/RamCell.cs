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

using System;
using System.IO;
using Xive.Mnemonic;
using Yaapii.Atoms;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which exists in memory.
    /// </summary>
    public sealed class RamCell : ICell
    {
        private readonly IScalar<string> name;
        private readonly IScalar<IMnemonic> mem;
        private readonly string id;

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell() : this(
            new Solid<string>(() => Guid.NewGuid().ToString()),
            new Solid<IMnemonic>(() => new RamMnemonic())
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(MemoryStream content) : this(
            new Live<string>(() => Guid.NewGuid().ToString()),
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
            new Live<string>(() => path),
            new Solid<IMnemonic>(() =>
            {
                var mem = new RamMnemonic();
                mem.Contents().UpdateBytes(path, content.AsBytes());
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
            new Solid<IMnemonic>(() =>
            {
                var mem = new RamMnemonic();
                mem.Contents()
                    .UpdateBytes(
                        path.Value(),
                        new BytesOf(
                            new InputOf(content)
                        ).AsBytes()
                    );
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
            new Live<string>(path),
            new Solid<IMnemonic>(() =>
            {
                var mem = new RamMnemonic();
                mem.Contents()
                    .UpdateBytes(
                        path,
                        new BytesOf(
                            new InputOf(content)
                        ).AsBytes()
                    );
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
        public RamCell(string path) : this(path, new RamMnemonic())
        { }

        /// <summary>
        /// An item which exists in memory.
        /// This cell has its internal memory stored in the itemMap. So: If you make two cells
        /// using this ctor, they will have the same memory.
        /// </summary>
        /// <param name="path">path to the item (key to the map)</param>
        /// <param name="cellMemory">map with content for many items</param>
        public RamCell(string path, IMnemonic mem) : this(
            new Live<string>(path),
            new Live<IMnemonic>(mem)
        )
        { }

        /// <summary>
        /// An item which exists in memory.
        /// This cell has its internal memory stored in the itemMap. So: If you make two cells
        /// using this ctor, they will have the same memory.
        /// </summary>
        /// <param name="path">path to the item (key to the map)</param>
        /// <param name="itemMap">map with content for many items</param>
        internal RamCell(IScalar<string> name, IScalar<IMnemonic> mem)
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
            return
                this.mem.Value()
                    .Contents()
                    .Bytes(
                        new Normalized(this.name.Value()).AsString(),
                        () => new byte[0]
                    );
        }

        public void Update(IInput content)
        {
            var name = new Normalized(this.name.Value()).AsString();
            var stream = content.Stream();
            if (stream.Length > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
                this.mem
                    .Value()
                    .Contents()
                    .UpdateBytes(
                        name,
                        new BytesOf(new InputOf(stream)).AsBytes()
                    );
            }
            else
            {
                this.mem.Value().Contents().UpdateBytes(name, new byte[0]);
            }
        }

        public void Dispose()
        {
            //Do nothing.
        }
    }
}
