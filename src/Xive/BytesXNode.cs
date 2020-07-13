using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive
{
    /// <summary>
    /// Bytes parsed as XNode.
    /// </summary>
    public sealed class BytesXNode : IScalar<XNode>
    {
        private readonly IScalar<XNode> node;

        /// <summary>
        /// Bytes parsed as XNode.
        /// </summary>
        public BytesXNode(string name, byte[] bytes)
        {
            this.node =
                new ScalarOf<XNode>(() =>
                {
                    XDocument doc;
                    if (bytes.Length == 0)
                    {
                        var rootName = new Normalized(name).AsString();
                        if (rootName.ToLower().EndsWith(".xml"))
                        {
                            rootName = rootName.Substring(0, rootName.Length - 4);
                        }
                        rootName = rootName.Substring(rootName.LastIndexOf("/"));
                        rootName = rootName.TrimStart('/');
                        doc =
                            new XDocument(
                                new XDeclaration("1.0", "UTF-8", "yes"),
                                new XElement(rootName)
                            );
                    }
                    else
                    {
                        try
                        {
                            using (var reader = new StreamReader(new MemoryStream(bytes)))
                            {
                                doc = XDocument.Load(reader);
                            }
                        }
                        catch (XmlException ex)
                        {
                            throw
                                new ApplicationException(
                                    $"Cannot parse this content as XML: '{new TextOf(new InputOf(bytes), Encoding.UTF8).AsString()}'",
                                    ex
                                );
                        }
                    }
                    return doc;
                });
        }

        public XNode Value()
        {
            return this.node.Value();
        }
    }
}
