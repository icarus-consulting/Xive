using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Xive.Comb;
using Yaapii.Atoms.Enumerable;

namespace Xive.Hive
{
    /// <summary>
    /// A cached hive.
    /// </summary>
    /// <param name="origin"></param>
    public sealed class CachedHive : IHive
    {
        private readonly IDictionary<string, byte[]> binMemory;
        private readonly IDictionary<string, XNode> xmlMemory;
        private readonly string name;
        private readonly IHive origin;

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live as long as this instance lives.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(string name, IHive origin) : this(name, origin, new Dictionary<string,byte[]>(), new Dictionary<string, XNode>())
        { }

        /// <summary>
        /// A cached hive.
        /// By using this ctor, the contents of the hive will live in the injected memories.
        /// </summary>
        /// <param name="origin"></param>
        public CachedHive(string name, IHive origin, IDictionary<string, byte[]> binMemory, IDictionary<string, XNode> xmlMemory)
        {
            this.binMemory = binMemory;
            this.xmlMemory = xmlMemory;
            this.origin = origin;
            this.name = name;
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return
                new Mapped<IHoneyComb, IHoneyComb>(
                    comb => 
                        new CachedComb(
                            $"{this.name}{Path.DirectorySeparatorChar}{comb.Name()}", 
                            comb, 
                            this.binMemory, 
                            this.xmlMemory
                        ),
                    this.origin.Combs(xpath)
                );
        }

        public IHoneyComb HQ()
        {
            return 
                new CachedComb(
                    this.origin.HQ().Name(), 
                    this.origin.HQ(), 
                    this.binMemory, 
                    this.xmlMemory
                );
        }

        public string Name()
        {
            return this.origin.Name();
        }
    }
}
