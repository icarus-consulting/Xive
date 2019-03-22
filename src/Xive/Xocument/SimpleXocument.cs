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
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    /// <summary>
    /// A simple Xocument.
    /// You can tell the Xocument what to to when it has been modified. 
    /// For example write its contents to a cell.
    /// </summary>
    public sealed class SimpleXocument : IXocument
    {
        private readonly IScalar<XNode> node;
        private readonly Action<XNode> update;

        /// <summary>
        /// A simple Xocument from a <see cref="XNode"/>
        /// </summary>
        /// <param name="node"></param>
        public SimpleXocument(XNode node) : this(
            node,
            update => { }
        )
        { }

        /// <summary>
        /// A simple Xocument.
        /// You can tell the Xocument what to to after it has been modified. 
        /// For example write its contents to a cell.
        /// </summary>
        public SimpleXocument(XNode node, Action<XNode> update) : this(new StickyScalar<XNode>(() => node), update)
        { }

        /// <summary>
        /// A simple Xocument.
        /// </summary>
        /// <param name="rootName"></param>
        public SimpleXocument(string rootName) : this(rootName, update => { })
        { }

        /// <summary>
        /// A simple Xocument.
        /// You can tell the Xocument what to to when it has been modified. 
        /// For example write its contents to a cell.
        /// </summary>
        public SimpleXocument(string rootName, Action<XNode> update) : this(
            new StickyScalar<XNode>(() =>
            {
                if (rootName.ToLower().EndsWith(".xml"))
                {
                    rootName = rootName.Substring(0, rootName.Length - 4);
                }
                return
                    new XDocument(
                        new XElement(rootName)
                    );
            }),
            update
        )
        { }

        /// <summary>
        /// A simple Xocument.
        /// You can tell the Xocument what to to when it has been modified. 
        /// For example write its contents to a cell.
        /// </summary>
        public SimpleXocument(IScalar<XNode> node, Action<XNode> update)
        {
            this.node = node;
            this.update = update;
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            var node = this.node.Value();
            new Xambler(dirs).ApplyQuietly(node);
            this.update(node);
        }

        public XNode Node()
        {
            return this.node.Value();
        }

        public IList<IXML> Nodes(string xpath)
        {
            return new XMLCursor(this.node.Value()).Nodes(xpath);
        }

        public string Value(string xpath, string def)
        {
            var values = this.Values(xpath);
            if (values.Count > 1)
            {
                throw new ArgumentException($"Cannot retrieve value with xpath '{xpath}' because it resulted in multiple values ({values.Count}) in document {this.node.Value().ToString()}");
            }

            string val;
            if (values.Count == 0)
            {
                val = def;
            }
            else
            {
                val = values[0];
            }
            return val;
        }

        public IList<string> Values(string xpath)
        {
            return new XMLCursor(Node()).Values(xpath);
        }

        public void Dispose()
        {
            //Do nothing.
        }
    }
}
