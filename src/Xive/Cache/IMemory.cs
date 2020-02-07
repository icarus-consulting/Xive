using System;
using System.IO;
using System.Xml.Linq;

namespace Xive.Hive
{
    /// <summary>
    /// A memory for all three types of data.
    /// </summary>
    public interface IMemory
    {
        void Update(string name, MemoryStream binary);
        void Update(string name, XNode xNode);
        MemoryStream Binary(string name, Func<MemoryStream> ifAbsent);
        XNode Xml(string name, Func<XNode> ifAbsent);
        IProps Props(string name, Func<IProps> ifAbsent);
    }
}
