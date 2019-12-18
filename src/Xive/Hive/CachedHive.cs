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
        private readonly ICache cache;
        private readonly IHive origin;

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live as long as this instance lives.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(IHive origin, int maxBytes = 10485760) : this(
            origin,
            new LimitedCache(maxBytes, new SimpleCache())
        )
        { }

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live as long as this instance lives.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(IHive origin, IList<string> blacklist, int maxBytes = 10485760) : this(
            origin,
            new BlacklistCache(
                blacklist,
                new LimitedCache(
                    maxBytes,
                    new SimpleCache()
                )
            )
        )
        { }


        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live in the injected memories.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(IHive origin, ICache cache)
        {
            this.cache = cache;
            this.origin = origin;
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return
                new Mapped<IHoneyComb, IHoneyComb>(
                    comb =>
                        new CachedComb(
                            comb,
                            this.cache
                        ),
                        this.origin.Combs(xpath, catalog => CachedCatalog())
                    );
        }

        public IEnumerable<IHoneyComb> Combs(string xpath, Func<ICatalog, ICatalog> catalogWrap)
        {
            return
                new Mapped<IHoneyComb, IHoneyComb>(
                    comb =>
                        new CachedComb(
                            comb,
                            this.cache
                        ),
                        this.origin.Combs(xpath, (catalog) => CachedCatalog())
                    );
        }

        public IHoneyComb HQ()
        {
            return
                new CachedComb(
                    this.origin.HQ(),
                    this.cache
                );
        }

        public IHive Shifted(string scope)
        {
            return
                new CachedHive(
                    this.origin.Shifted(scope),
                    this.cache
                );
        }

        public string Scope()
        {
            return this.origin.Scope();
        }

        private ICatalog CachedCatalog()
        {
            return
                new SimpleCatalog(
                    this.origin.Scope(),
                    this.HQ()
                );
        }
    }
}
