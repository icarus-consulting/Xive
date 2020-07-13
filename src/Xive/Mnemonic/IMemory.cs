using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Xive.Mnemonic
{
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