using System;
using System.Collections.Generic;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Scalar;
using Xive.Comb;

namespace Xive.Hive
{
    /// <summary>
    /// A hive that accesses cells systemwide exclusively.
    /// </summary>
    public sealed class SyncHive : IHive
    {
        private readonly IHive hive;

        /// <summary>
        /// A hive that accesses cells systemwide exclusively.
        /// </summary>
        public SyncHive(IHive hive)
        {
            this.hive = hive;
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            lock (hive)
            {
                return new Mapped<IHoneyComb, IHoneyComb>((comb) => new SyncComb(comb), hive.Combs(xpath));
            }
        }

        public IHoneyComb HQ()
        {
            lock (hive)
            {
                return new SyncComb(hive.HQ());
            }
        }

        public string Name()
        {
            lock (hive)
            {
                return hive.Name();
            }
        }
    }
}
