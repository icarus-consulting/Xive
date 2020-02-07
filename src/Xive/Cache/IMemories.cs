using System.IO;
using System.Xml.Linq;
using Xive.Hive;

namespace Xive.Cache
{
    public interface IMemories
    {
        IMemory<IProps> Props();
        IMemory<XNode> XML();
        IMemory<MemoryStream> Data();
    }
}
