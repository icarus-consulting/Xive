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

using System.Collections.Generic;
using Xive.Comb;
using Xive.Mnemonic;
using Xive.Xocument;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.List;
using Yaapii.Xambly;

namespace Xive.Hive
{
    /// <summary>
    /// The index of a xive, realized as a catalog xml document.
    /// </summary>
    public sealed class XiveIndex : IIndex
    {
        private readonly string scope;
        private readonly ISyncValve valve;
        private readonly IMnemonic mem;
        private readonly List<string> idCache;

        /// <summary>
        /// The index of a xive, realized as a catalog xml document.
        /// </summary>
        public XiveIndex(string scope, IMnemonic mem, ISyncValve valve)
        {
            this.mem = mem;
            this.valve = valve;
            this.scope = scope;
            this.idCache = new List<string>();
        }

        public void Add(string id)
        {
            using (var xoc = ExclusiveXoc())
            {
                xoc.Modify(
                    new Directives()
                        .Xpath("/catalog")
                        .Append(
                            new EnumerableOf<IDirective>(
                                new AddIfAttributeDirective(scope, "id", id.ToLower())
                            )
                        )
                    );
                lock (idCache)
                {
                    idCache.Clear();
                    idCache.AddRange(xoc.Values("/catalog/*/@id"));
                }
            }
        }

        public IList<IHoneyComb> List(params IHiveFilter[] filters)
        {
            IList<IHoneyComb> filtered = new List<IHoneyComb>();
            var fltrs = new List<IHiveFilter>(filters);
            lock (idCache)
            {
                if (idCache.Count == 0)
                {
                    idCache.AddRange(ExclusiveXoc().Values("/catalog/*/@id"));
                }
            }
            foreach(var id in idCache)
            {
                if (filters.Length == 0)
                {
                    filtered.Add(
                        new MemorizedComb(
                            new Normalized($"{scope.ToLower()}/{id.ToLower()}").AsString(), 
                            this.mem
                        )
                    );
                }
                else
                {
                    foreach (var filter in fltrs)
                    {
                        if (filter.Matches(this.mem.Props(scope, id)))
                        {
                            filtered.Add(
                                new MemorizedComb(
                                    new Normalized($"{scope}/{id}").AsString(), 
                                    this.mem
                                )
                            );
                        }
                    }
                }
            }
            return new ListOf<IHoneyComb>(filtered);
        }

        public bool Has(string id)
        {
            lock (idCache)
            {
                if (idCache.Count == 0)
                {
                    idCache.AddRange(ExclusiveXoc().Values("/catalog/*/@id"));
                }
            }
            return idCache.Contains(id.ToLower());
        }

        public void Remove(string id)
        {
            using (var xoc = ExclusiveXoc())
            {
                xoc.Modify(
                    new Directives()
                        .Xpath($"/catalog/{this.scope.ToLower()}[@id='{id.ToLower()}']")
                        .Remove()
                );
                lock (idCache)
                {
                    idCache.Clear();
                    idCache.AddRange(xoc.Values("/catalog/*/@id"));
                }
            }
        }

        private IXocument ExclusiveXoc()
        {
            return
                new SyncXocument(
                    $"{scope}/hq/catalog.xml",
                    new MemorizedXocument($"{scope}/hq/catalog.xml", this.mem),
                    valve
                );
        }

        //private IXocument Xoc()
        //{
        //    return new MemorizedXocument($"{scope}/hq/catalog.xml", this.mem);
        //}

    }
}
