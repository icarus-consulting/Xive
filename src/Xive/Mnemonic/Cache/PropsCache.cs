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
using System.Collections.Concurrent;

namespace Xive.Mnemonic.Cache
{
    /// <summary>
    /// Cached props
    /// </summary>
    public sealed class PropsCache : ICache<ConcurrentDictionary<string, string[]>>
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string[]>> propsMem;

        /// <summary>
        /// Cached props
        /// </summary>
        public PropsCache()
        {
            this.propsMem = new ConcurrentDictionary<string, ConcurrentDictionary<string, string[]>>();
        }

        public void Clear()
        {
            lock (this.propsMem)
            {
                this.propsMem.Clear();
            }
        }

        public ConcurrentDictionary<string, string[]> Content(string name, Func<ConcurrentDictionary<string, string[]>> ifAbsent)
        {
            lock (this.propsMem)
            {
                return this.propsMem.GetOrAdd(
                       name,
                       (key) => ifAbsent()
                   );
            }
        }

        public void Remove(string name)
        {
            lock (this.propsMem)
            {
                this.propsMem.TryRemove(name, out ConcurrentDictionary<string, string[]> props);
            }
        }

        public void Update(string name, Func<ConcurrentDictionary<string, string[]>> ifAbsent, Func<ConcurrentDictionary<string, string[]>> ifExists)
        {
            lock (this.propsMem)
            {
                this.propsMem.AddOrUpdate(name, (key) => ifAbsent(), (key, old) => ifExists());
            }
        }
    }
}
