//MIT License

//Copyright (c) 2019 ICARUS Consulting GmbH

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
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    /// <summary>
    /// A Xocument whose data is cached.
    /// If data is updated, cache and origin are updated too.
    /// </summary>
    public sealed class CachedXocument : IXocument
    {
        private readonly string name;
        private readonly IXocument origin;
        private readonly IDictionary<string, XNode> xmlMemory;

        /// <summary>
        /// A Xocument whose data is cached.
        /// If data is updated, cache and origin are updated too.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="origin"></param>
        /// <param name="xmlMemory"></param>
        public CachedXocument(string name, IXocument origin, IDictionary<string, XNode> xmlMemory)
        {
            this.name = name;
            this.origin = origin;
            this.xmlMemory = xmlMemory;
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            this.origin.Modify(dirs);
            this.xmlMemory[this.name] = this.origin.Node();
        }

        public IList<IXML> Nodes(string xpath)
        {
            return Cached().Nodes(xpath);
        }

        public string Value(string xpath, string def)
        {
            return Cached().Value(xpath, def);
        }

        public IList<string> Values(string xpath)
        {
            return Cached().Values(xpath);
        }

        public XNode Node()
        {
            if (!this.xmlMemory.ContainsKey(this.name))
            {
                this.xmlMemory.Add(this.name, this.origin.Node());
            }
            return this.xmlMemory[this.name];
        }

        public void Dispose()
        {
            this.origin.Dispose();
        }

        private IXocument Cached()
        {
            return new SimpleXocument(this.Node());
        }
    }
}
