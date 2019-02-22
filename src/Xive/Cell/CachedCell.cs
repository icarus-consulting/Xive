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
            else
            {
                this.memory.Remove(this.name);
            }
        }

        public void Dispose()
        {
            //Do nothing.
        }
    }
}