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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memory for XML nodes.
    /// </summary>
    public sealed class DataRam : IMemory<byte[]>
    {
        private readonly ConcurrentDictionary<string, byte[]> mem;

        /// <summary>
        /// Memory for XML nodes.
        /// </summary>
        public DataRam() : this(new ConcurrentDictionary<string, byte[]>())
        { }

        /// <summary>
        /// Memory for XML nodes.
        /// </summary>
        public DataRam(ConcurrentDictionary<string, byte[]> mem)
        {
            this.mem = mem;
        }

        public bool Knows(string name)
        {
            return this.mem.ContainsKey(name);
        }

        public IEnumerable<string> Knowledge()
        {
            return this.mem.Keys;
        }

        public byte[] Content(string name, Func<byte[]> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var result = this.mem.GetOrAdd(name, (key) => ifAbsent());
            return result;
        }

        public void Update(string name, byte[] content)
        {
            name = new Normalized(name).AsString();
            if (content.Length == 0)
            {
                byte[] removed;
                this.mem.TryRemove(name, out removed);
            }
            else
            {
                this.mem.AddOrUpdate(name, content, (currentName, currentContent) => content);
            }
        }
    }
}
