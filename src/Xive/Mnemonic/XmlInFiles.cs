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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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
