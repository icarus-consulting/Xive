using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    public sealed class ReadOnlyXocument : IXocument
    {
        private readonly IXML xml;

        public ReadOnlyXocument(IXML xml)
        {
            this.xml = xml;
        }

        public void Dispose()
        { }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            throw new InvalidOperationException("Cant modify the Xocument, because it is read only.");
        }

        public XNode Node()
        {
            return this.xml.AsNode();
        }

        public IList<IXML> Nodes(string xpath)
        {
            return this.xml.Nodes(xpath);
        }

        public string Value(string xpath, string def)
        {
            var result = def;
            var results = this.xml.Values(xpath);
            if (results.Count > 0)
            {
                result = results[0];
            }
            return result;
        }

        public IList<string> Values(string xpath)
        {
            return this.xml.Values(xpath);
        }
    }
}
