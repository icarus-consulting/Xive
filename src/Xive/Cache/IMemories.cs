using System.IO;
using System.Xml.Linq;
using Xive.Hive;

namespace Xive.Cache
{
    public interface IMemories
    {
        IProps Props(string scope, string id);
        IMemory<XNode> XML();
        IMemory<MemoryStream> Data();
    }
}
