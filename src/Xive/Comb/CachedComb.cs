using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Xive.Cell;
using Xive.Xocument;

namespace Xive.Comb
{
    /// <summary>
    /// A comb that is cached in memory.
    /// </summary>
    public sealed class CachedComb : IComb
    {
        private readonly IDictionary<string, byte[]> binMemory;
        private readonly IDictionary<string, XNode> xmlMemory;
        private readonly string name;
        private readonly IComb comb;

        /// <summary>
        /// A comb that is cached in memory.
        /// </summary>
        public CachedComb(string name, IComb comb, IDictionary<string, byte[]> binMemory, IDictionary<string, XNode> xmlMemory)
        {
            this.name = name;
            this.comb = comb;
            this.binMemory = binMemory;
            this.xmlMemory = xmlMemory;
        }

        public ICell Cell(string name)
        {
            return 
                new CachedCell(
                    this.comb.Cell(name), 
                    $"{this.name}{Path.DirectorySeparatorChar}{name}",
                    this.binMemory
                );
        }

        public string Name()
        {
            return this.Name();
        }

        public IXocument Xocument(string name)
        {
            return
                new CachedXocument(
                    $"{this.name}{Path.DirectorySeparatorChar}{name}",
                    this.comb.Xocument(name),
                    this.xmlMemory
                );
        }
    }
}
