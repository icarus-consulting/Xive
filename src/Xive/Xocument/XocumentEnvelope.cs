//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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

using System.Collections.Generic;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    /// <summary>
    /// Envelope for xocument.
    /// </summary>
    public class XocumentEnvelope : IXocument
    {
        private readonly IScalar<IXocument> origin;

        /// <summary>
        /// Envelope for xocument.
        /// </summary>
        public XocumentEnvelope(IScalar<IXocument> origin)
        {
            this.origin = origin;
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            this.origin.Value().Modify(dirs);
        }

        public IList<IXML> Nodes(string xpath)
        {
            return this.origin.Value().Nodes(xpath);
        }

        public string Value(string xpath, string def)
        {
            return this.origin.Value().Value(xpath, def);
        }

        public IList<string> Values(string xpath)
        {
            return this.origin.Value().Values(xpath);
        }

        public XNode Node()
        {
            return this.origin.Value().Node();
        }

        public void Dispose()
        {
            this.origin.Value().Dispose();
        }
    }
}
