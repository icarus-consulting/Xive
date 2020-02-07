using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Xive.Hive;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic
{
    public sealed class XmlInFiles : IMemory<XNode>
    {
        private DataInFiles memory;

        public XmlInFiles(string root)
        {
            this.memory = new DataInFiles(root);
        }

        public XNode Content(string name, Func<XNode> ifAbsent)
        {
            var data = 
                this.memory.Content(name, () =>
                {
                    var node = ifAbsent();
                    return new MemoryStream(new BytesOf(node.ToString()).AsBytes());
                });
            return Parsed(name, data);

        }

        public IEnumerable<string> Knowledge()
        {
            return this.memory.Knowledge();
        }

        public bool Knows(string name)
        {
            return this.memory.Knows(name);
        }

        public void Update(string name, XNode content)
        {
            if (!content.Document.Root.HasElements)
            {
                this.memory.Update(name, new MemoryStream());
            }
            else
            {
                this.memory.Update(
                    name,
                    new MemoryStream(
                        new BytesOf(
                            Encoding.UTF8.GetBytes(content.ToString())
                        ).AsBytes()
                    )
                );
            }
        }

        private XNode Parsed(string name, MemoryStream data)
        {
            XDocument doc;
            if (data.Length == 0)
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
                    doc =
                        XDocument.Parse(
                            new TextOf(
                                new InputOf(data),
                                Encoding.UTF8
                            ).AsString()
                        );
                }
                catch (XmlException ex)
                {
                    throw
                        new ApplicationException(
                            $"Cannot parse this content as XML: '{new TextOf(new InputOf(data), Encoding.UTF8).AsString()}'",
                            ex
                        );
                }
            }
            return doc;
        }
    }
}
