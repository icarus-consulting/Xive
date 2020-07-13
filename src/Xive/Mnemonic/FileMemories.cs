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

using System.Xml.Linq;
using Xive.Cache;
using Xive.Mnemonic.Content;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memories in Files.
    /// </summary>
    public sealed class FileMemories : IMnemonic
    {
        private readonly SimpleMemories mem;

        /// <summary>
        /// Memories in Files.
        /// </summary>
        /// <param name="writeAsync">if true, when updating a memory, the file is written asynchronously. 
        /// Use this in combination with a cache, because the cache is up-to-date, no matter when the file is written.</param>
        public FileMemories(string root, bool writeAsync = false) : this(root, new LocalSyncPipe())
        { }

        /// <summary>
        /// Memories in Files.
        /// </summary>
        /// <param name="writeAsync">if true, when updating a memory, the file is written asynchronously. 
        /// Use this in combination with a cache, because the cache is up-to-date, no matter when the file is written.</param>
        public FileMemories(string root, ISyncPipe pipe, bool writeAsync = false)
        {
            this.mem =
                new SimpleMemories(
                    new XmlFileMemory(root, pipe, writeAsync),
                    new DataFileMemory(root, pipe, writeAsync)
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
    /// Memories in Files.
    /// </summary>
    public sealed class FileMemories2 : IMnemonic2
    {
        private readonly SimpleMemories2 mem;

        /// <summary>
        /// Memories in Files.
        /// </summary>
        /// <param name="writeAsync">if true, when updating a memory, the file is written asynchronously. 
        /// Use this in combination with a cache, because the cache is up-to-date, no matter when the file is written.</param>
        public FileMemories2(string root, bool writeAsync = false) : this(root, new LocalSyncPipe())
        { }

        /// <summary>
        /// Memories in Files.
        /// </summary>
        /// <param name="writeAsync">if true, when updating a memory, the file is written asynchronously. 
        /// Use this in combination with a cache, because the cache is up-to-date, no matter when the file is written.</param>
        public FileMemories2(string root, ISyncPipe pipe, bool writeAsync = false)
        {
            this.mem =
                new SimpleMemories2(
                    new FileContents(root, pipe, writeAsync)
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
