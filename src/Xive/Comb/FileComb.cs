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

using Xive.Mnemonic;

namespace Xive.Comb
{
    /// <summary>
    /// A comb that is stored in a file.
    /// </summary>
    public sealed class FileComb : IHoneyComb
    {
        private readonly MemorizedComb core;

        /// <summary>
        /// A comb that is stored in a file.
        /// </summary>
        public FileComb(string root, string name) : this(name, new FileMnemonic(root))
        { }

        /// <summary>
        /// A comb that is stored in a file.
        /// </summary>
        internal FileComb(string name, IMnemonic mem)
        {
            this.core = new MemorizedComb(name, mem);
        }

        public ICell Cell(string name)
        {
            return this.core.Cell(name);
        }

        public string Name()
        {
            return this.core.Name();
        }

        public IProps Props()
        {
            return this.core.Props();
        }

        public IXocument Xocument(string name)
        {
            return this.core.Xocument(name);
        }
    }
}
