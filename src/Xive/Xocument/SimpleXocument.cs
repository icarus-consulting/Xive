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
        public SimpleXocument(XNode node) : this(node, update => { })
        { }

        /// <summary>
        /// A simple Xocument.
        /// You can tell the Xocument what to to when it has been modified. 
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
                if(rootName.ToLower().EndsWith(".xml"))
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
            return new XMLQuery(this.node.Value()).Nodes(xpath);
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
            return new XMLQuery(Node()).Values(xpath);
        }

        public void Dispose()
        {
            //Do nothing.
        }
    }
}
