using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Cache;
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
        private readonly IMemories memories;
        private readonly Sticky<XNode> node;
        private readonly string name;

        /// <summary>
        /// A xocument stored in memories.
        /// </summary>
        public MemorizedXocument(string name, IMemories memories)
        {
            this.memories = memories;
            this.node =
                new Sticky<XNode>(() =>
                {
                    var node =
                        memories.XML().Content(name, () =>
                        {
                            var rootName = new Normalized(name).AsString();
                            if (rootName.ToLower().EndsWith(".xml"))
                            {
                                rootName = rootName.Substring(0, rootName.Length - 4);
                            }
                            rootName = rootName.Substring(rootName.LastIndexOf("/"));
                            rootName = rootName.TrimStart('/');
                            return
                                new XDocument(
                                    new XElement(rootName)
                                );
                        });
                    return node;
                });
            this.name = name;
        }

        public void Dispose()
        { }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            var node = this.node.Value();
            new Xambler(dirs).ApplyQuietly(node);
            this.memories.XML().Update(this.name, node);
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
