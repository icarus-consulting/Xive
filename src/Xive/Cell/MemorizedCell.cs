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
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell whose content is stored in memory.
    /// </summary>
    public sealed class MemorizedCell : ICell
    {
        private readonly IText name;
        private readonly Sticky<IMnemonic> mem;

        public MemorizedCell(string name, byte[] data, IMnemonic mem) : this(name, new Sticky<IMnemonic>(() =>
            {
                mem.Data().Update(name, data);
                return mem;
            })
        )
        { }

        /// <summary>
        /// A cell whose content is stored in memory.
        /// </summary>
        public MemorizedCell(string name, IMnemonic mem) : this(name, new Sticky<IMnemonic>(mem))
        { }

        /// <summary>
        /// A cell whose content is stored in memory.
        /// </summary>
        private MemorizedCell(string name, Sticky<IMnemonic> mem)
        {
            this.name = new Normalized(name);
            this.mem = mem;
        }

        public string Name()
        {
            return this.name.AsString();
        }

        public byte[] Content()
        {
            return
                this.mem.Value().Data().Content(
                    this.name.AsString(),
                    () => new byte[0]
                );
        }

        public void Update(IInput content)
        {
            var stream = content.Stream();
            stream.Seek(0, SeekOrigin.Begin);
            var data =
                new BytesOf(
                    new InputOf(stream)
                ).AsBytes();
            this.mem
                .Value()
                .Data()
                .Update(
                    this.name.AsString(),
                    data
                );
        }

        public void Dispose()
        { }
    }
}