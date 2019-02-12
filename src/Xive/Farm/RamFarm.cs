using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Hive;

namespace Xive.Farm
{
    /// <summary>
    /// A farm which lives in memory.
    /// </summary>
    public sealed class RamFarm : IFarm
    {
        private readonly Func<string, IHive> hive;
        private readonly IDictionary<string, IDictionary<string, byte[]>> farmMemory;

        /// <summary>
        /// A farm which lives in memory. The contents will live as long as this instance lives.
        /// </summary>
        public RamFarm() : this(new Dictionary<string, IDictionary<string, byte[]>>())
        { }

        /// <summary>
        /// A farm which lives in memory. The contents will live as long as this instance lives.
        /// Using this ctor, you can tell the farm, how its hives should build a catalog.
        /// </summary>
        public RamFarm(Func<string, IHoneyComb, ICatalog> catalog) : this(comb => comb, hive => hive, catalog, new Dictionary<string, IDictionary<string, byte[]>>())
        { }

        /// <summary>
        /// A farm which lives in memory. The contents will live as long as the injected Dictionary lives.
        /// </summary>
        public RamFarm(IDictionary<string, IDictionary<string, byte[]>> memory) : this(hiveName =>
            {
                if (!memory.ContainsKey(hiveName))
                {
                    memory.Add(hiveName, new Dictionary<string, byte[]>());
                }
                return new RamHive(hiveName, comb => comb, (name, hq) => new Catalog(hiveName, hq), memory[hiveName]);
            },
            memory
        )
        { }

        /// <summary>
        /// A farm which lives in memory. The contents will live as long as the injected Dictionary lives.
        /// Using this ctor, this farm is rather dumb: You must tell it how to build a catalog.
        /// You can use combWrap and/or hiveWrap to wrap if necessary.
        /// </summary>
        public RamFarm(Func<IHive, IHive> hiveWrap, Func<IHoneyComb, IHoneyComb> combWrap, Func<string, IHoneyComb, ICatalog> catalog, IDictionary<string, IDictionary<string, byte[]>> memory) : this(hiveName =>
             {
                 if (!memory.ContainsKey(hiveName))
                 {
                     memory.Add(hiveName, new Dictionary<string, byte[]>());
                 }
                 return hiveWrap(new RamHive(hiveName, comb => combWrap(comb), catalog, memory[hiveName]));
             },
            memory
        )
        { }

        internal RamFarm(Func<string, IHive> hive, IDictionary<string, IDictionary<string, byte[]>> rawMemory)
        {
            this.hive = hive;
            this.farmMemory = rawMemory;
        }

        public IHive Hive(string name)
        {
            return this.hive(name);
        }
    }
}
