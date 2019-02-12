using System;
using System.Collections.Generic;
using System.Text;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;

namespace Xive.Test.Cell
{
    /// <summary>
    /// A cell whose content is cached in memory.
    /// </summary>
    public sealed class CachedCell : ICell
    {
        private readonly ICell origin;
        private readonly string name;
        private readonly IDictionary<string, byte[]> memory;

        /// <summary>
        /// A cell whose content is cached in memory.
        /// </summary>
        public CachedCell(ICell origin, string name, IDictionary<string, byte[]> memory)
        {
            this.origin = origin;
            this.name = name;
            this.memory = memory;
        }

        public byte[] Content()
        {
            if (!this.memory.ContainsKey(this.name))
            {
                this.memory[this.name] = origin.Content();
            }
            return this.memory[this.name];
        }

        public void Update(IInput content)
        {
            this.origin.Update(content);
            var stream = content.Stream();
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            this.memory[this.name] = new BytesOf(new InputOf(stream)).AsBytes();
        }

        public void Dispose()
        {
            //Do nothing.
        }
    }
}