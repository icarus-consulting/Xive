using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memorized content.
    /// </summary>
    public interface IContent
    {
        IList<string> Knowledge();
        void UpdateXml(string name, XNode xml);
        void UpdateBytes(string name, byte[] data);
        XNode Xml(string name, Func<XNode> ifAbsent);
        byte[] Bytes(string name, Func<byte[]> ifAbsent);
    }
}
