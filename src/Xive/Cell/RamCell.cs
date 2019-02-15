using System;
using System.Collections.Generic;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which exists in memory.
    /// </summary>
    public sealed class RamCell : ICell
    {
        private readonly IScalar<IDictionary<string, byte[]>> cellMemory;
        private readonly IScalar<string> name;
        private readonly string id;

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell() : this(
            new StickyScalar<string>(() => Guid.NewGuid().ToString()),
            new ScalarOf<IDictionary<string, byte[]>>(new Dictionary<string, byte[]>())
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(byte[] content) : this(
            new StickyScalar<string>(() => Guid.NewGuid().ToString()),
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
            new StickyScalar<string>(() => path),
            new StickyScalar<IDictionary<string, byte[]>>(
                () => new Dictionary<string, byte[]>()
                {
                    { path, content.AsBytes() }
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
        public RamCell(IScalar<string> path, byte[] content) : this(
            path,
            new StickyScalar<IDictionary<string, byte[]>>(
                () => new Dictionary<string, byte[]> { { path.Value(), content } }
            )
        )
        { }

        /// <summary>
        /// A cell which exists in memory.
        /// Note: This memory cell lives only as long as this object lives.
        /// If you create two cells with the same path using this ctor,
        /// they will not have the same content.
        /// </summary>
        public RamCell(string path, byte[] content) : this(
            new ScalarOf<string>(path),
            new ScalarOf<IDictionary<string, byte[]>>(() =>
            {
                return new Dictionary<string, byte[]>()
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
        public RamCell(string path) : this(path, new Dictionary<string, byte[]>())
        { }

        /// <summary>
        /// An item which exists in memory.
        /// This cell has its internal memory stored in the itemMap. So: If you make two cells
        /// using this ctor, they will have the same memory.
        /// </summary>
        /// <param name="path">path to the item (key to the map)</param>
        /// <param name="cellMemory">map with content for many items</param>
        public RamCell(string path, IDictionary<string, byte[]> cellMemory) : this(
            new ScalarOf<string>(path),
            new ScalarOf<IDictionary<string, byte[]>>(cellMemory)
        )
        { }

        /// <summary>
        /// An item which exists in memory.
        /// This cell has its internal memory stored in the itemMap. So: If you make two cells
        /// using this ctor, they will have the same memory.
        /// </summary>
        /// <param name="path">path to the item (key to the map)</param>
        /// <param name="itemMap">map with content for many items</param>
        internal RamCell(IScalar<string> name, IScalar<IDictionary<string, byte[]>> cellMemory)
        {
            
                this.id = Guid.NewGuid().ToString();
                this.name = 
                    new StickyScalar<string>(
                        () => new StrictCellName(name.Value()).AsString()
                    );
                this.cellMemory = 
                    new ScalarOf<IDictionary<string, byte[]>>(() =>
                        {
                            lock (cellMemory)
                            {
                                var memory = cellMemory.Value();
                                if (!memory.ContainsKey(name.Value()))
                                {
                                    memory[name.Value()] = new byte[0];
                                }
                                return memory;
                            }
                        }
                    );
        }

        public byte[] Content()
        {
            return this.cellMemory.Value()[this.name.Value()];
        }

        public void Update(IInput content)
        {
            byte[] bytes = new BytesOf(content).AsBytes();
            this.cellMemory.Value()[this.name.Value()] = bytes;
        }

        public void Dispose()
        {
            //Do nothing.
        }
    }
}
