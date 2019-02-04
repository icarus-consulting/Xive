using System;
using System.Collections.Generic;
using System.IO;
using Yaapii.Atoms;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Scalar;

namespace Xive.Hive
{
    /// <summary>
    /// A simple hive.
    /// This hive is rather dumb: You need to tell it how to build a comb and how to build a catalog.
    /// </summary>
    public class SimpleHive : IHive
    {
        private readonly IText name;
        private readonly Func<string, IComb> comb;
        private readonly IScalar<ICatalog> catalog;

        /// <summary>
        /// A simple hive.
        /// Using this ctor, this hive is rather dumb: You need to tell it how to build a comb.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="comb">How to build a comb: (combName) => new SomeCatalog(...)</param>
        public SimpleHive(string name, Func<string, IComb> comb) : this(
            name,
            comb,
            (hiveName, catalogComb) => new Catalog(hiveName, catalogComb)
        )
        { }

        /// <summary>
        /// A simple hive.
        /// Using this ctor, this hive is rather dumb: You need to tell it how to build a comb and how to build a catalog.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="comb">How to build a comb: (combName) => new SomeCatalog(...)</param>
        /// <param name="hiveCatalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        public SimpleHive(string name, Func<string, IComb> comb, Func<string, IComb, ICatalog> catalog)
        {
            this.name = new Coordinate(name);
            this.comb = comb;
            this.catalog = new StickyScalar<ICatalog>(() => catalog(name, HQ()));
        }

        public IComb HQ()
        {
            return this.comb($"{this.name.AsString()}{Path.DirectorySeparatorChar}HQ");
        }

        public IEnumerable<IComb> Combs(string xpath)
        {
            return
                new Mapped<string, IComb>(
                    comb => 
                    {
                        return this.comb($"{this.name.AsString()}{Path.DirectorySeparatorChar}{comb}");
                    },
                    this.catalog.Value().List(xpath)
                );
        }

        public string Name()
        {
            return this.name.AsString();
        }
    }
}
