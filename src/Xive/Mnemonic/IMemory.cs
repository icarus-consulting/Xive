using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Xive.Mnemonic
{
    /// <summary>
    /// A memory for all three types of data.
    /// </summary>
    public interface ICache
    {
        void Update(string name, MemoryStream binary);
        void Update(string name, XNode xNode);
        MemoryStream Binary(string name, Func<MemoryStream> ifAbsent);
        XNode Xml(string name, Func<XNode> ifAbsent);
        IProps Props(string name, Func<IProps> ifAbsent);
    }

    /// <summary>
    /// A memory for all three types of data.
    /// </summary>
    public interface IMemory<T>
    {
        bool Knows(string name);
        IEnumerable<string> Knowledge();
        void Update(string name, T content);
        T Content(string name, Func<T> ifAbsent);
    }
}