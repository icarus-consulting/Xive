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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Xmls stored in files.
    /// </summary>
    public sealed class XmlInFiles : IMemory<XNode>
    {
        private DataInFiles memory;

        /// <summary>
        /// Xmls stored in files.
        /// </summary>
        public XmlInFiles(string root) : this(root, new LocalSyncPipe())
        { }

        /// <summary>
        /// Xmls stored in files.
        /// </summary>
        /// <param name="root">if true, when updating, the file is written asynchronously.</param>
        public XmlInFiles(string root, ISyncPipe sync, bool writeAsync = false)
        {
            this.memory = new DataInFiles(root, sync, writeAsync);
        }

        public XNode Content(string name, Func<XNode> ifAbsent)
        {
            XNode result;
            if (!this.memory.Knows(name))
            {
                result = ifAbsent();
                if (!result.Document.Root.IsEmpty)
                {
                    Update(name, result);
                }
            }
            else
            {
                result = Parsed(name, this.memory.Content(name, () => throw new ApplicationException($"Internal error, assumend to never access ifAbsent() method here.")));
            }
            return result;

        }

        public IEnumerable<string> Knowledge()
        {
            return this.XmlKnowledge();
        }

        public bool Knows(string name)
        {
            return this.memory.Knows(name);
        }

        public void Update(string name, XNode content)
        {
            if (content.Document.Root.IsEmpty)
            {
                this.memory.Update(name, new byte[0]);
            }
            else
            {
                this.memory.Update(
                    name,
                    new BytesOf(
                        Encoding.UTF8.GetBytes(content.ToString())
                    ).AsBytes()
                );
            }
        }

        private IEnumerable<string> XmlKnowledge()
        {
            var xmls = new List<string>();
            foreach (var data in this.memory.Knowledge())
            {
                var content = this.memory.Content(data, () => new byte[0]);
                if (content.Length > 0)
                {
                    var filename = System.IO.Path.GetFileNameWithoutExtension(data);

                    var headtext = new TextOf(new HeadOf<byte>(content, 100).ToArray()).AsString();
                    var root = $"<{filename}";
                    if (headtext.IndexOf(root) >= 0)
                    {
                        xmls.Add(data);
                    }
                    else if (
                        headtext.IndexOf("<?xml", StringComparison.InvariantCultureIgnoreCase) >= 0 &&
                        headtext.IndexOf("?>", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        xmls.Add(data);
                    }
                }
            }
            return xmls;
        }

        private XNode Parsed(string name, byte[] data)
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
                    using (var reader = new StreamReader(new MemoryStream(data)))
                    {
                        doc = XDocument.Load(reader);
                    }
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
