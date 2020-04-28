using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Mnemonic;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    /// <summary>
    /// A xocument stored in memories.
    /// </summary>
    public sealed class MemorizedXocument : IXocument
    {
        private readonly IMnemonic memories;
        private readonly IScalar<XNode> node;
        private readonly IScalar<string> root;
        private readonly string name;

        /// <summary>
        /// A xocument stored in memories.
        /// </summary>
        public MemorizedXocument(string name, IMnemonic memories)
        {
            this.name = name;
            this.memories = memories;
            this.node =
                new Live<XNode>(() =>
                    memories.XML().Content(name, () =>
                        new XDocument(
                            new XElement(this.root.Value())
                        )
                    )
                );
            this.root =
                new ScalarOf<string>(() =>
                {
                    var rootName = new Normalized(name).AsString();
                    if (rootName.ToLower().EndsWith(".xml"))
                    {
                        rootName = rootName.Substring(0, rootName.Length - 4);
                    }
                    if (rootName.Contains("/"))
                    {
                        rootName = rootName.Substring(rootName.LastIndexOf("/"));
                        rootName = rootName.TrimStart('/');
                    }
                    return rootName;
                });
        }

        public void Dispose()
        { }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            var copy = new XDocument(this.node.Value().Document.Root);
            new Xambler(dirs).ApplyQuietly(copy);
            this.memories.XML().Update(this.name, copy);
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
    }
}
