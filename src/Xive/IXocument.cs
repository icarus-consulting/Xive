//MIT License

//Copyright (c) 2020 ICARUS Consulting GmbH

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive
{
    /// <summary>
    /// A cross xml document: It combines reading XML and updating XML using Xambly.
    /// </summary>
    public interface IXocument : IDisposable
    {
        /// <summary>
        /// Get values by xpath.
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        IList<string> Values(string xpath);

        /// <summary>
        /// Get single value by xpath, or default.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        string Value(string xpath, string def);

        /// <summary>
        /// Get nodes by xpath (XMLCursors https://github.com/icarus-consulting/Yaapii.Xml).
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        IList<IXML> Nodes(string xpath);

        /// <summary>
        /// Modify the XML with Xambly directives.
        /// https://github.com/icarus-consulting/Yaapii.Xambly
        /// </summary>
        /// <param name="dirs"></param>
        void Modify(IEnumerable<IDirective> dirs);

        /// <summary>
        /// The xnode representation.
        /// </summary>
        /// <returns></returns>
        XNode Node();
    }
}
