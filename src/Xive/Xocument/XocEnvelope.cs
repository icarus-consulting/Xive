using System.Collections.Generic;
using Yaapii.Atoms;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    public class XocEnvelope : IXocument
    {
        private readonly IScalar<IXocument> origin;

        public XocEnvelope(IScalar<IXocument> origin)
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
    }
}
