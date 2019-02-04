using System.Collections.Generic;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive
{
    public interface IXocument
    {
        IList<string> Values(string xpath);
        string Value(string xpath, string def);
        IList<IXML> Nodes(string xpath);
        void Modify(IEnumerable<IDirective> dirs);
    }
}
