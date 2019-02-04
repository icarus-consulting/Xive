using System;
using Xive.Hive;

namespace Xive.Farm
{
    /// <summary>
    /// A farm that accesses cells systemwide exclusively.
    /// </summary>
    public sealed class SyncFarm : IFarm
    {
        private readonly IFarm farm;

        public SyncFarm(IFarm farm)
        {
            this.farm = farm;
        }

        /// <summary>
        /// Access a specific hive.
        /// Example: 
        /// Farm(maybe for: An App "Organizer") -> Hive(maybe: Calendar) -> n Comb(maybe: Appointment) -> n Cells(maybe: subscribers.xml)
        /// </summary>
        /// <param name="name">Name of the hive</param>
        /// <returns>The hive</returns>
        public IHive Hive(string name)
        {
            return new SyncHive(farm.Hive(name));
        }
    }
}
