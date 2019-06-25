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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;
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
        private readonly IScalar<IDictionary<string, XNode>> xmlMemory;
        private readonly IList<string> blacklist;

        /// <summary>
        /// A Xocument whose data is cached.
        /// If data is updated, cache and origin are updated too.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="origin"></param>
        /// <param name="xmlMemory"></param>
        public CachedXocument(string name, IXocument origin, IDictionary<string, XNode> xmlMemory, IDictionary<string, MemoryStream> binMemory) : this(
            name, origin, xmlMemory, binMemory, new List<string>()
        )
        { }

        /// <summary>
        /// A Xocument whose data is cached.
        /// If data is updated, cache and origin are updated too.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="origin"></param>
        /// <param name="xmlMemory"></param>
        public CachedXocument(string name, IXocument origin, IDictionary<string, XNode> xmlMemory, IDictionary<string, MemoryStream> binMemory, IList<string> blacklist)
        {
            this.blacklist = blacklist;
            this.name = name;
            this.origin = origin;
            this.xmlMemory =
                new ScalarOf<IDictionary<string, XNode>>(() =>
                {
                    if (binMemory.ContainsKey(this.name))
                    {
                        throw
                            new InvalidOperationException(
                                $"Cannot use '{this.name}' as Xocument because it has previously been used as cell. "
                                + " Caching only works with consistent usage of contents."
                            );
                    }
                    return xmlMemory;
                });
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            this.origin.Modify(dirs);
            if (!Blacklisted(this.name))
            {
                this.xmlMemory.Value()[this.name] = this.origin.Node();
            }
        }

        public IList<IXML> Nodes(string xpath)
        {
            return Content().Nodes(xpath);
        }

        public string Value(string xpath, string def)
        {
            return Content().Value(xpath, def);
        }

        public IList<string> Values(string xpath)
        {
            return Content().Values(xpath);
        }

        public XNode Node()
        {
            XNode result;
            if (!this.xmlMemory.Value().ContainsKey(this.name) || Blacklisted(this.name))
            {
                result = this.origin.Node();
                if (!Blacklisted(this.name))
                {
                    this.xmlMemory.Value().Add(this.name, result);
                }
            }
            else
            {
                result = this.xmlMemory.Value()[this.name];
            }
            return result;
        }

        public void Dispose()
        {
            this.origin.Dispose();
        }

        private bool Blacklisted(string name)
        {
            bool blacklisted = false;
            foreach (var entry in this.blacklist)
            {
                var pattern = 
                    Regex.Escape(entry.ToLower()).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                blacklisted = Regex.IsMatch(name.ToLower(), pattern);
                if (blacklisted)
                    break;
            }
            return blacklisted;
        }

        private IXocument Content()
        {
            return new SimpleXocument(this.Node());
        }
    }
}
