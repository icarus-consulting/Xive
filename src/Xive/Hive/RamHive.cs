using System;
using System.Collections.Generic;
using Yaapii.Atoms.Scalar;
using Xive.Comb;

namespace Xive.Hive
{
    /// <summary>
    /// A hive that lives in memory.
    /// </summary>
    public sealed class RamHive : HiveEnvelope
    {
        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, this hive's contents will live only as long as this instance lives.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        public RamHive(string name) : this(
            name, 
            (hiveName, comb) => new Catalog(hiveName, comb)
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, you can construct the hive multiple times with the same name and memory
        /// and its combs will always have the same contents.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="memory">An external memory for the hive</param>
        public RamHive(string name, IDictionary<string, byte[]> memory) : this(
            name,
            (hiveName, comb) => new Catalog(hiveName, comb),
            memory
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, this hive's contents will live only as long as this instance
        /// lives.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="catalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        public RamHive(string name, Func<string, IComb, ICatalog> catalog): this(
            name, 
            catalog, 
            new Dictionary<string, byte[]>()
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, you can construct the hive multiple times with the same name and its combs will
        /// always have the same contents.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="catalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        /// <param name="memory">An external memory for the hive</param>
        public RamHive(string name, Func<string, IComb, ICatalog> catalog, IDictionary<string, byte[]> memory) : base(
            new StickyScalar<IHive>(() =>
              {
                  return 
                    new SimpleHive(
                        name,
                        comb => new RamComb(comb, memory),
                        catalog
                    );
              })
        )
        { }
    }
}
