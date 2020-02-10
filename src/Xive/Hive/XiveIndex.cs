using System;
using System.Collections.Generic;
using Xive.Cache;
using Xive.Comb;
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
        private readonly IXocument xoc;
        private readonly ISyncValve valve;
        private readonly IMemories mem;

        /// <summary>
        /// The index of a xive, realized as a catalog xml document.
        /// </summary>
        public XiveIndex(string scope, IMemories mem, ISyncValve valve)
        {
            this.mem = mem;
            this.valve = valve;
            this.scope = scope;
            this.xoc = new MemorizedXocument($"{scope}/hq/catalog.xml", mem);
        }

        public void Add(string id)
        {
            using (var xoc = this.xoc)
            {
                if(this.xoc.Nodes($"/catalog/{this.scope}[@id='{id.ToLower()}']").Count > 0)
                {
                    throw new InvalidOperationException($"Cannot add {id} to {scope} catalog because it already exists.");
                }
                this.xoc.Modify(new Directives().Xpath("/catalog").Add(scope).Attr("id", id.ToLower()));
            }
        }

        public IList<IHoneyComb> Filtered(params IIndexFilter[] filters)
        {
            IList<IHoneyComb> filtered = new List<IHoneyComb>();
            var fltrs = new List<IIndexFilter>(filters);
            foreach(var id in this.xoc.Values("/catalog/*/@id"))
            {
                foreach(var filter in fltrs)
                {
                    if(filter.Matches(this.mem.Props(scope, id)))
                    {
                        filtered.Add(new MemorizedComb($"{scope}/{id}", this.mem));
                    }
                }
            }
            return new ListOf<IHoneyComb>(filtered);
        }

        public bool Has(string id)
        {
            return this.xoc.Nodes($"/catalog/{this.scope}[@id='{id.ToLower()}']").Count > 0;
        }

        public void Remove(string id)
        {
            using (var xoc = this.xoc)
            {
                this.xoc.Modify(new Directives().Xpath($"/catalog/{this.scope.ToLower()}[@id='{id.ToLower()}']").Remove());
            }
        }

        private IXocument Exclusive()
        {
            return
                new SyncXocument(
                    $"{scope}/hq/catalog.xml",
                    this.xoc,
                    valve
                );
        }
    }
}
