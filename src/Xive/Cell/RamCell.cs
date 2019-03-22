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
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which exists in memory.
    /// </summary>
    public sealed class RamCell : ICell
    {
        private readonly IScalar<IDictionary<string, MemoryStream>> cellMemory;
        private readonly IScalar<string> name;
        private readonly string id;

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell() : this(
            new SolidScalar<string>(() => Guid.NewGuid().ToString()),
            new SolidScalar<IDictionary<string, MemoryStream>>(() => new Dictionary<string, MemoryStream>())
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
            new SolidScalar<IDictionary<string, MemoryStream>>(
                () => new Dictionary<string, MemoryStream>()
                {
                    { path, new MemoryStream(content.AsBytes()) }
                }
            )
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
            new SolidScalar<IDictionary<string, MemoryStream>>(
                () => new Dictionary<string, MemoryStream> { { path.Value(), content } }
            )
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
            new SolidScalar<IDictionary<string, MemoryStream>>(() =>
            {
                return new Dictionary<string, MemoryStream>()
                {
                    { path, content }
                };
            })
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells with the same path using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(string path) : this(path, new Dictionary<string, MemoryStream>())
        { }

        /// <summary>
        /// An item which exists in memory.
        /// This cell has its internal memory stored in the itemMap. So: If you make two cells
        /// using this ctor, they will have the same memory.
        /// </summary>
        /// <param name="path">path to the item (key to the map)</param>
        /// <param name="cellMemory">map with content for many items</param>
        public RamCell(string path, IDictionary<string, MemoryStream> cellMemory) : this(
            new ScalarOf<string>(path),
            new ScalarOf<IDictionary<string, MemoryStream>>(cellMemory)
        )
        { }

        /// <summary>
        /// An item which exists in memory.
        /// This cell has its internal memory stored in the itemMap. So: If you make two cells
        /// using this ctor, they will have the same memory.
        /// </summary>
        /// <param name="path">path to the item (key to the map)</param>
        /// <param name="itemMap">map with content for many items</param>
        internal RamCell(IScalar<string> name, IScalar<IDictionary<string, MemoryStream>> cellMemory)
        {
            lock (cellMemory)
            {
                this.id = Guid.NewGuid().ToString();
                this.name =
                    new SolidScalar<string>(
                        () => new StrictCellName(name.Value()).AsString()
                    );
                this.cellMemory = cellMemory;
            }
        }

        public string Name()
        {
            return this.name.Value();
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];
            if (this.cellMemory.Value().ContainsKey(name.Value()))
            {
                this.cellMemory.Value()[this.name.Value()].Seek(0, SeekOrigin.Begin);
                result = this.cellMemory.Value()[this.name.Value()].ToArray();
            }
            return result;
        }

        public void Update(IInput content)
        {
            lock (cellMemory)
            {
                var stream = content.Stream();
                if (stream.Length > 0)
                {
                    var memory = new MemoryStream();
                    content.Stream().CopyTo(memory);
                    memory.Seek(0, SeekOrigin.Begin);
                    content.Stream().Seek(0, SeekOrigin.Begin);
                    this.cellMemory.Value()[this.name.Value()] = memory;
                }
                else
                {
                    this.cellMemory.Value().Remove(this.name.Value());
                }
            }
        }

        public void Dispose()
        {
            //Do nothing.
        }
    }
}
