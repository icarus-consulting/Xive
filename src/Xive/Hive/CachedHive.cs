//MIT License

//Copyright (c) 2019 ICARUS Consulting GmbH

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

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Xive.Comb;
using Yaapii.Atoms.Enumerable;

namespace Xive.Hive
{
    /// <summary>
    /// A cached hive.
    /// </summary>
    /// <param name="origin"></param>
    public sealed class CachedHive : IHive
    {
        private readonly IDictionary<string, byte[]> binMemory;
        private readonly IDictionary<string, XNode> xmlMemory;
        private readonly string name;
        private readonly IHive origin;

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live as long as this instance lives.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(string name, IHive origin) : this(name, origin, new Dictionary<string,byte[]>(), new Dictionary<string, XNode>())
        { }

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live in the injected memories.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(string name, IHive origin, IDictionary<string, byte[]> binMemory, IDictionary<string, XNode> xmlMemory)
        {
            this.binMemory = binMemory;
            this.xmlMemory = xmlMemory;
            this.origin = origin;
            this.name = name;
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return
                new Mapped<IHoneyComb, IHoneyComb>(
                    comb => 
                        new CachedComb(
                            $"{this.name}{Path.DirectorySeparatorChar}{comb.Name()}", 
                            comb, 
                            this.binMemory, 
                            this.xmlMemory
                        ),
                    this.origin.Combs(xpath)
                );
        }

        public IHoneyComb HQ()
        {
            return 
                new CachedComb(
                    this.origin.HQ().Name(), 
                    this.origin.HQ(), 
                    this.binMemory, 
                    this.xmlMemory
                );
        }

        public IHive Shifted(string scope)
        {
            return this.origin.Shifted(scope);
        }

        public string Scope()
        {
            return this.origin.Scope();
        }
    }
}
