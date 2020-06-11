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

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memory for anything.
    /// You must tell the memory how it can check if the given object is empty, because it will automatically remove these items.
    /// </summary>
    public sealed class VersatileRamMemory<T> : IMemory<T>
    {
        private readonly ConcurrentDictionary<string, T> mem;
        private readonly Func<T, bool> isEmpty;

        /// <summary>
        /// Memory for anything.
        /// You must tell the memory how it can check if the given object is empty, because it will automatically remove these items.
        /// </summary>
        public VersatileRamMemory(Func<T, bool> isEmpty) : this(new ConcurrentDictionary<string, T>(), isEmpty)
        { }

        /// <summary>
        /// Memory for anything.
        /// You must tell the memory how it can check if the given object is empty, because it will automatically remove these items.
        /// </summary>
        public VersatileRamMemory(ConcurrentDictionary<string, T> mem, Func<T, bool> isEmpty)
        {
            this.mem = mem;
            this.isEmpty = isEmpty;
        }

        public bool Knows(string name)
        {
            return this.mem.ContainsKey(name);
        }

        public IEnumerable<string> Knowledge()
        {
            return this.mem.Keys;
        }

        public T Content(string name, Func<T> ifAbsent)
        {
            name = new Normalized(name).AsString();
            return this.mem.GetOrAdd(name, (key) => ifAbsent());
        }

        public void Update(string name, T content)
        {
            name = new Normalized(name).AsString();
            if (this.isEmpty(content))
            {
                T removed;
                this.mem.TryRemove(name, out removed);
            }
            else
            {
                this.mem.AddOrUpdate(name, content, (currentName, currentContent) => content);
            }
        }
    }
}
