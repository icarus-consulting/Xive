using System.Collections.Generic;
using Yaapii.Atoms;

namespace Xive.Hive
{
    /// <summary>
    /// Envelope for simple hive building.
    /// </summary>
    public abstract class HiveEnvelope : IHive
    {
        private readonly IScalar<IHive> hive;

        /// <summary>
        /// Envelope for simple hive building.
        /// </summary>
        public HiveEnvelope(IScalar<IHive> hive)
        {
            this.hive = hive;
        }

        public IEnumerable<IComb> Combs(string xpath)
        {
            return this.hive.Value().Combs(xpath);
        }

        public IComb HQ()
        {
            return this.hive.Value().HQ();
        }

        public string Name()
        {
            return this.hive.Value().Name();
        }
    }
}
