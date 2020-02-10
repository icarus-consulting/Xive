using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xive.Comb;
using Xive.Mnemonic;
using Xive.Xocument;
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
        private readonly IMemories mem;
        private readonly List<string> idCache;

        /// <summary>
        /// The index of a xive, realized as a catalog xml document.
        /// </summary>
        public XiveIndex(string scope, IMemories mem, ISyncValve valve)
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
                if(xoc.Nodes($"/catalog/{this.scope}[@id='{id.ToLower()}']").Count > 0)
                {
                    throw new InvalidOperationException($"Cannot add {id} to {scope} catalog because it already exists.");
                }
                xoc.Modify(
                    new Directives()
                        .Xpath("/catalog")
                        .Add(scope)
                        .Attr("id", id.ToLower())
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
                    idCache.AddRange(Xoc().Values("/catalog/*/@id"));
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
                    idCache.AddRange(Xoc().Values("/catalog/*/@id"));
                }
            }
            return Xoc().Nodes($"/catalog/{this.scope}[@id='{id.ToLower()}']").Count > 0;
        }

        public void Remove(string id)
        {
            using (var xoc = Xoc())
            {
                Xoc().Modify(
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
                    Xoc(),
                    valve
                );
        }

        private IXocument Xoc()
        {
            return new MemorizedXocument($"{scope}/hq/catalog.xml", this.mem);
        }

    }
}
