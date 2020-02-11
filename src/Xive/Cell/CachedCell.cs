//MIT License

//Copyright (c) 2020 ICARUS Consulting GmbH

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
using Xive.Mnemonic;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which is cached in memory.
    /// </summary>
    public sealed class CachedCell : ICell
    {
        private readonly ICell origin;
        private readonly IMnemonic mem;

        /// <summary>
        /// A cell which is cached in memory.
        /// </summary>
        public CachedCell(ICell origin) : this(origin, new RamMemories())
        { }

        /// <summary>
        /// A cell which is cached in memory.
        /// </summary>
        internal CachedCell(ICell origin, IMnemonic cache)
        {
            this.origin = origin;
            this.mem = cache;

        }

        public byte[] Content()
        {
            return
                this.mem
                    .Data()
                    .Content(
                        this.origin.Name(),
                        () => new MemoryStream(this.origin.Content())
                    ).ToArray();
        }

        public string Name()
        {
            return this.origin.Name();
        }

        public void Update(IInput content)
        {
            var stream = content.Stream();
            if (stream.Length > 0)
            {
                var memory = new MemoryStream();
                content.Stream().CopyTo(memory);
                memory.Seek(0, SeekOrigin.Begin);
                content.Stream().Seek(0, SeekOrigin.Begin);
                this.mem.Data().Update(this.origin.Name(), memory);
            }
            else
            {
                this.mem
                    .Data()
                    .Update(this.origin.Name(), new MemoryStream());
            }
            this.origin.Update(
                new InputOf(
                    this.mem
                        .Data()
                        .Content(this.origin.Name(), () => new MemoryStream())
                    )
                );
        }

        public void Dispose()
        { }
    }
}
