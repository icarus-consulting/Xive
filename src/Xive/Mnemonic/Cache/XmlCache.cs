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
using System.Collections.Generic;
using System.Xml.Linq;
using Yaapii.Atoms.Enumerable;

namespace Xive.Mnemonic.Cache
{
    /// <summary>
    /// A cache for xml.
    /// </summary>
    public sealed class XmlCache : ICache<XNode>
    {
        ConcurrentDictionary<string, XNode> memory;

        /// <summary>
        /// A cache for xml.
        /// </summary>
        public XmlCache(params KeyValuePair<string, XNode>[] contents) : this(new ManyOf<KeyValuePair<string, XNode>>(contents))
        { }

        /// <summary>
        /// A cache for xml.
        /// </summary>
        public XmlCache(IEnumerable<KeyValuePair<string, XNode>> contents)
        {
            this.memory = new ConcurrentDictionary<string, XNode>(contents);
        }

        public void Clear()
        {
            lock (this.memory)
            {
                this.memory.Clear();
            }
        }

        public XNode Content(string name, Func<XNode> ifAbsent)
        {
            lock (this.memory)
            {
                return
                    this.memory
                        .GetOrAdd(name, (n) => ifAbsent());
            }
        }

        public void Remove(string name)
        {
            XNode unused;
            this.memory.TryRemove(name, out unused);
        }

        public void Update(string name, Func<XNode> ifAbsent, Func<XNode> ifExisting)
        {
            lock (this.memory)
            {
                this.memory
                    .AddOrUpdate(
                       name,
                       (n) => ifAbsent(),
                       (n, existing) => ifExisting()
                    );
            }
        }
    }
}
