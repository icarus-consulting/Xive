using System;
using System.IO;
using System.Xml.Linq;

namespace Xive.Hive
{
    /// <summary>
    /// A cache which can contain multiple
    /// </summary>
    public interface ICache
    {
        bool Has(string name);
        void Update(string name, MemoryStream binary);
        void Update(string name, XNode xNode);
        MemoryStream Binary(string name, Func<MemoryStream> ifAbsent);
        XNode Xml(string name, Func<XNode> ifAbsent);
    }
}
