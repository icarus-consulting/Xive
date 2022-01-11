//MIT License

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

using System.IO;
using Xive.Mnemonic;
using Xive.Mnemonic.Content;
using Yaapii.Atoms;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which is cached in memory.
    /// </summary>
    public sealed class CachedCell : ICell
    {
        private readonly ICell origin;
        private readonly IContents mem;

        /// <summary>
        /// A cell which is cached in memory.
        /// </summary>
        public CachedCell(ICell origin) : this(origin, new RamContents())
        { }

        /// <summary>
        /// A cell which is cached in memory.
        /// </summary>
        internal CachedCell(ICell origin, IContents cache)
        {
            this.origin = origin;
            this.mem = cache;

        }

        public byte[] Content()
        {
            return
                this.mem
                    .Bytes(
                        this.origin.Name(),
                        () => this.origin.Content()
                    );
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
                stream.Seek(0, SeekOrigin.Begin);
                this.mem
                    .UpdateBytes(
                        this.origin.Name(),
                        new BytesOf(new InputOf(stream)).AsBytes()
                    );
            }
            else
            {
                this.mem
                    .UpdateBytes(this.origin.Name(), new byte[0]);
            }
            this.origin.Update(
                new InputOf(
                    this.mem
                        .Bytes(this.origin.Name(), () => new byte[0])
                )
            );
        }

        public void Dispose()
        { }
    }
}
