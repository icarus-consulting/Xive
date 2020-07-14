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
using System.Threading;
using System.Xml.Linq;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    /// <summary>
    /// A Xocument which is accessed exclusively.
    /// </summary>
    public sealed class SyncXocument : IXocument
    {
        private readonly string name;
        private readonly IXocument origin;
        private readonly ISyncPipe sync;
        private readonly int[] locked;

        /// <summary>
        /// A Xocument which is accessed exclusively.
        /// </summary>
        public SyncXocument(string name, IXocument origin, ISyncPipe sync)
        {
            lock (this)
            {
                this.name = name;
                this.origin = origin;
                this.sync = sync;
            }
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            this.sync.Flush(this.name, () => this.origin.Modify(dirs));
        }

        public XNode Node()
        {
            XNode result = new XDocument();
            this.sync.Flush(this.name, () => result = this.origin.Node());
            return result;
        }

        public IList<IXML> Nodes(string xpath)
        {
            IList<IXML> result = new List<IXML>();
            this.sync.Flush(this.name, () => result = this.origin.Nodes(xpath));
            return result;
        }

        public string Value(string xpath, string def)
        {
            string result = String.Empty;
            this.sync.Flush(this.name, () => result = this.origin.Value(xpath, def));
            return result;
        }

        public IList<string> Values(string xpath)
        {
            IList<string> result = new List<string>();
            this.sync.Flush(this.name, () => this.origin.Values(xpath));
            return result;
        }

        public void Dispose()
        {

        }
    }
}
