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

using System;
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
        private readonly IDictionary<string, MemoryStream> binMemory;
        private readonly IDictionary<string, XNode> xmlMemory;
        private readonly IList<string> blacklist;
        private readonly IHive origin;
        private readonly int maxBytes;

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live as long as this instance lives.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(IHive origin, int maxBytes = 10485760) : this(
            origin,
            new Dictionary<string, MemoryStream>(),
            new Dictionary<string, XNode>(),
            new List<string>(),
            maxBytes
        )
        { }

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live as long as this instance lives.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(IHive origin, IList<string> blacklist, int maxBytes = 10485760) : this(
            origin,
            new Dictionary<string, MemoryStream>(),
            new Dictionary<string, XNode>(),
            blacklist,
            maxBytes
        )
        { }

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live in the injected memories.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(IHive origin, IDictionary<string, MemoryStream> binMemory, IDictionary<string, XNode> xmlMemory, int maxBytes = 10485760) : this(
            origin,
            binMemory,
            xmlMemory,
            new List<string>(),
            maxBytes
        )
        { }


        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live in the injected memories.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(IHive origin, IDictionary<string, MemoryStream> binMemory, IDictionary<string, XNode> xmlMemory, IList<string> blacklist, int maxBytes = 10485760)
        {
            this.binMemory = binMemory;
            this.xmlMemory = xmlMemory;
            this.origin = origin;
            this.maxBytes = maxBytes;
            this.blacklist = blacklist;
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return
                new Mapped<IHoneyComb, IHoneyComb>(
                    comb =>
                        new CachedComb(
                            comb,
                            this.binMemory,
                            this.xmlMemory,
                            this.blacklist,
                            this.maxBytes
                        ),
                        this.origin.Combs(xpath, CachedCatalog())
                    );
        }

        public IEnumerable<IHoneyComb> Combs(string xpath, ICatalog catalog)
        {
            throw new InvalidOperationException($"Using a cached hive while providing a custom catalog is not supported.");
        }

        public IHoneyComb HQ()
        {
            return
                new CachedComb(
                    this.origin.HQ(),
                    this.binMemory,
                    this.xmlMemory,
                    this.blacklist
                    this.maxBytes
                );
        }

        public IHive Shifted(string scope)
        {
            return
                new CachedHive(
                    this.origin.Shifted(scope),
                    this.binMemory,
                    this.xmlMemory,
                    this.maxBytes
                );
        }

        public string Scope()
        {
            return this.origin.Scope();
        }

        private ICatalog CachedCatalog()
        {
            return
                new MutexCatalog(
                    this.origin.Scope(),
                    new CachedComb(
                        this.origin.HQ(),
                        this.binMemory,
                        this.xmlMemory,
                        this.blacklist,
                        this.maxBytes
                    )
                );
        }
    }
}
