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
using Yaapii.Atoms.Enumerable;

namespace Xive.Mnemonic.Cache
{
    /// <summary>
    /// A cache for bytes.
    /// </summary>
    public sealed class BytesCache : ICache<byte[]>
    {
        private readonly long maxSize;
        ConcurrentDictionary<string, byte[]> memory;

        /// <summary>
        /// A cache for bytes.
        /// </summary>
        public BytesCache(long maxSize = Int64.MaxValue) : this(
            new ManyOf<KeyValuePair<string, byte[]>>(),
            maxSize
        )
        { }

        /// <summary>
        /// A cache for bytes.
        /// </summary>
        public BytesCache(params KeyValuePair<string, byte[]>[] contents) : this(
            new ManyOf<KeyValuePair<string, byte[]>>(contents),
            Int64.MaxValue
        )
        { }

        /// <summary>
        /// A cache for bytes.
        /// </summary>
        public BytesCache(IEnumerable<KeyValuePair<string, byte[]>> contents, long maxSize = Int64.MaxValue)
        {
            this.memory = new ConcurrentDictionary<string, byte[]>(contents);
            this.maxSize = maxSize;
        }

        public byte[] Content(string name, Func<byte[]> ifAbsent)
        {
            byte[] result;
            lock (this.memory)
            {
                if (!this.memory.ContainsKey(name))
                {
                    result = ifAbsent();
                    if (result.Length <= this.maxSize)
                    {
                        this.memory
                            .GetOrAdd(name, (n) => result);
                    }
                }
                else
                {
                    result = this.memory.GetOrAdd(name, (n) => ifAbsent());
                }
            }
            return result;

        }

        public void Remove(string name)
        {
            byte[] unused;
            this.memory.TryRemove(name, out unused);
        }

        public void Update(string name, Func<byte[]> ifAbsent, Func<byte[]> ifExisting)
        {
            lock (this.memory)
            {
                byte[] data;
                if (!this.memory.ContainsKey(name))
                {
                    data = ifAbsent();
                    if (data.Length <= this.maxSize)
                    {
                        this.memory[name] = data;
                    }
                }
                else
                {
                    data = ifExisting();
                    if (data.Length <= this.maxSize)
                    {
                        this.memory[name] = data;
                    }
                }
            }
        }
    }
}
