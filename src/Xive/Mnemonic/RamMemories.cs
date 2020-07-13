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
using System.Xml.Linq;
using Xive.Cache;
using Xive.Hive;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memories in Ram.
    /// </summary>
    public sealed class RamMemories : IMnemonic
    {
        private readonly IMnemonic mem;

        /// <summary>
        /// Memories in Ram.
        /// </summary>
        public RamMemories()
        {
            this.mem = 
                new SimpleMemories(
                    new XmlRamMemory(), 
                    new DataRamMemory()
                );
        }

        public IMemory<byte[]> Data()
        {
            return this.mem.Data();
        }

        public IProps Props(string scope, string id)
        {
            return this.mem.Props(scope, id);
        }

        public IMemory<XNode> XML()
        {
            return this.mem.XML();
        }
    }

    /// <summary>
    /// Memories in Ram.
    /// </summary>
    public sealed class RamMemories2 : IMnemonic2
    {
        private readonly IMnemonic2 mem;

        /// <summary>
        /// Memories in Ram.
        /// </summary>
        public RamMemories2()
        {
            this.mem =
                new SimpleMemories2(
                    new RamContents()
                );
        }

        public IProps Props(string scope, string id)
        {
            return this.mem.Props(scope, id);
        }

        public IContents Contents()
        {
            return this.mem.Contents();
        }
    }
}
