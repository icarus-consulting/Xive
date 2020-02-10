using System;
using System.Collections.Concurrent;
using Xive.Comb;
using Xive.Mnemonic;

namespace Xive.Hive
{
    /// <summary>
    /// A hive which is memorized in the given memories.
    /// </summary>
    public sealed class MemorizedHive : IHive
    {
        private readonly string scope;
        private readonly ConcurrentDictionary<string, IIndex> indices;
        private readonly ISyncValve valve;
        private readonly IMemories mem;

        /// <summary>
        /// A hive which is memorized in the given memories.
        /// </summary>
        public MemorizedHive(string scope, IMemories memories) : this(scope, memories, new ConcurrentDictionary<string, IIndex>(), new SyncGate())
        { }

        /// <summary>
        /// A hive which is memorized in the given memories.
        /// </summary>
        public MemorizedHive(string scope, IMemories memories, ConcurrentDictionary<string, IIndex> indices, ISyncValve valve)
        {
            this.scope = scope;
            this.mem = memories;
            this.indices = indices;
            this.valve = valve;
        }

        public IIndex Catalog()
        {
            return this.indices.GetOrAdd(scope.ToLower(), new XiveIndex(scope.ToLower(), this.mem, this.valve));
        }

        public IHoneyComb Comb(string id, bool createIfAbsent = true)
        {
            if(!Catalog().Has(id.ToLower()))
            {
                if (createIfAbsent)
                {
                    Catalog().Add(id.ToLower());
                }
                else
                {
                    throw new ArgumentException($"Cannot access a non existing comb with id '{id.ToLower()}'");
                }
            }
            return new MemorizedComb(new Normalized($"{this.scope}/{id}").AsString(), this.mem);
        }

        public IHoneyComb HQ()
        {
            return new MemorizedComb(new Normalized($"{this.scope}/hq").AsString(), this.mem);
        }

        public string Scope()
        {
            return this.scope;
        }

        public IHive Shifted(string scope)
        {
            return new MemorizedHive(scope.ToLower(), this.mem, this.indices, this.valve);
        }
    }
}
