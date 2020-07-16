using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memorized content.
    /// </summary>
    public interface IContents
    {
        /// <summary>
        /// Lists all content paths, that start with the given filter string
        /// </summary>
        /// <returns></returns>
        IList<string> Knowledge(string filter = "");
        void UpdateXml(string name, XNode xml);
        void UpdateBytes(string name, byte[] data);
        XNode Xml(string name, Func<XNode> ifAbsent);
        byte[] Bytes(string name, Func<byte[]> ifAbsent);
    }
}
