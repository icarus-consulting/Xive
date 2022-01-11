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

using System;
using Yaapii.Atoms;

namespace Xive.Test
{
    /// <summary>
    /// A fake cell.
    /// </summary>
    public sealed class FkCell : ICell
    {
        private readonly Func<byte[]> content;
        private readonly Action<IInput> update;
        private readonly string name;

        /// <summary>
        /// A fake cell.
        /// </summary>
        public FkCell(string name) : this(name, content => { }, () => new byte[0])
        { }

        /// <summary>
        /// A fake cell.
        /// </summary>
        public FkCell() : this("unknown", content => { }, () => new byte[0])
        { }

        /// <summary>
        /// A fake cell.
        /// </summary>
        public FkCell(Action<IInput> update, Func<byte[]> content) : this("unknown", update, content)
        { }

        /// <summary>
        /// A fake cell.
        /// </summary>
        public FkCell(string name, Action<IInput> update, Func<byte[]> content)
        {
            this.name = name;
            this.content = content;
            this.update = update;
        }

        public byte[] Content()
        {
            return this.content();
        }

        public void Update(IInput content)
        {
            this.update(content);
        }

        public void Dispose()
        { }

        public string Name()
        {
            return this.name;
        }
    }
}
