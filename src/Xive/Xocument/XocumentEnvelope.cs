using System.Collections.Generic;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    public class XocumentEnvelope : IXocument
    {
        private readonly IScalar<IXocument> origin;

        public XocumentEnvelope(IScalar<IXocument> origin)
        {
            this.origin = origin;
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            this.origin.Value().Modify(dirs);
        }

        public IList<IXML> Nodes(string xpath)
        {
            return this.origin.Value().Nodes(xpath);
        }

        public string Value(string xpath, string def)
        {
            return this.origin.Value().Value(xpath, def);
        }

        public IList<string> Values(string xpath)
        {
            return this.origin.Value().Values(xpath);
        }

        public XNode Node()
        {
            return this.origin.Value().Node();
        }

        public void Dispose()
        {
            this.origin.Value().Dispose();
        }
    }
}
