using System.Collections.Generic;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;

namespace Xive.Cell
{
    /// <summary>
    /// A cell whose content is cached in memory.
    /// </summary>
    public sealed class CachedCell : ICell
    {
        private readonly ICell origin;
        private readonly string name;
        private readonly IDictionary<string, byte[]> memory;
        private readonly int maxSize;

        /// <summary>
        /// A cell whose content is cached in memory.
        /// </summary>
        public CachedCell(ICell origin, string name, IDictionary<string, byte[]> memory, int maxBytes = 10485760)
        {
            this.origin = origin;
            this.name = name;
            this.memory = memory;
            this.maxSize = maxBytes;
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];
            if (!this.memory.ContainsKey(this.name))
            {
                result = this.origin.Content();
                if (result.Length <= this.maxSize)
                {
                    this.memory[this.name] = result;
                }
            }
            else
            {
                result = this.memory[this.name];
            }
            return result;
        }

        public void Update(IInput content)
        {
            this.origin.Update(content);
            var stream = content.Stream();

            if (stream.Length <= this.maxSize)
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                this.memory[this.name] = new BytesOf(new InputOf(stream)).AsBytes();
            }
        }

        public void Dispose()
        {
            //Do nothing.
        }
    }
}