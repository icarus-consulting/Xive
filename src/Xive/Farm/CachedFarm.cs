using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Hive;

namespace Xive.Farm
{
    /// <summary>
    /// A farm that is cached.
    /// </summary>
    public class CachedFarm : IFarm
    {
        private readonly IFarm origin;
        private readonly IDictionary<string, byte[]> binMemory;
        private readonly IDictionary<string, XNode> xmlMemory;

        /// <summary>
        /// A farm that is cached.
        /// </summary>
        /// <param name="origin"></param>
        public CachedFarm(IFarm origin) : this(origin, new Dictionary<string,byte[]>(), new Dictionary<string,XNode>())
        { }

        /// <summary>
        /// A farm that is cached.
        /// </summary>
        public CachedFarm(IFarm origin, IDictionary<string,byte[]> binMemory, IDictionary<string,XNode> xmlMemory)
        {
            this.origin = origin;
            this.binMemory = binMemory;
            this.xmlMemory = xmlMemory;
        }

        public IHive Hive(string name)
        {
            return
                new CachedHive(
                    new Coordinate(name).AsString(),
                    this.origin.Hive(name),
                    this.binMemory,
                    this.xmlMemory
                );
        }
    }
}
