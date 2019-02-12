using System.Collections.Generic;
using System.Xml.Linq;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    /// <summary>
    /// A Xocument whose data is cached.
    /// If data is updated, cache and origin are updated too.
    /// </summary>
    public sealed class CachedXocument : IXocument
    {
        private readonly string name;
        private readonly IXocument origin;
        private readonly IDictionary<string, XNode> xmlMemory;

        /// <summary>
        /// A Xocument whose data is cached.
        /// If data is updated, cache and origin are updated too.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="origin"></param>
        /// <param name="xmlMemory"></param>
        public CachedXocument(string name, IXocument origin, IDictionary<string, XNode> xmlMemory)
        {
            this.name = name;
            this.origin = origin;
            this.xmlMemory = xmlMemory;
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            this.origin.Modify(dirs);
            this.xmlMemory[this.name] = this.origin.Node();
        }

        public IList<IXML> Nodes(string xpath)
        {
            return Cached().Nodes(xpath);
        }

        public string Value(string xpath, string def)
        {
            return Cached().Value(xpath, def);
        }

        public IList<string> Values(string xpath)
        {
            return Cached().Values(xpath);
        }

        public XNode Node()
        {
            if (!this.xmlMemory.ContainsKey(this.name))
            {
                this.xmlMemory.Add(this.name, this.origin.Node());
            }
            return this.xmlMemory[this.name];
        }

        public void Dispose()
        {
            this.origin.Dispose();
        }

        private IXocument Cached()
        {
            return new SimpleXocument(this.Node());
        }
    }
}
