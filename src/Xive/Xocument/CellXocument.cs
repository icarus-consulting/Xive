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

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Xocument
{
    /// <summary>
    /// A xocument which is stored in a cell.
    /// </summary>
    public sealed class CellXocument : XocumentEnvelope
    {
        /// <summary>
        /// A xocument which is stored in a cell.
        /// </summary>
        public CellXocument(ICell cell, string name) : this(
            cell,
            name,
            Encoding.Default
        )
        {}

        /// <summary>
        /// A xocument which is stored in a cell.
        /// </summary>
        public CellXocument(ICell cell, string name, Encoding encoding) : base(new ScalarOf<IXocument>(() =>
            {
                lock (cell)
                {
                    return
                        new SimpleXocument(
                            new ScalarOf<XNode>(() =>
                            {
                                using (cell)
                                {
                                    if (name.ToLower().EndsWith(".xml"))
                                    {
                                        name = name.Substring(0, name.Length - 4);
                                    }
                                    name = Path.GetFileName(name);
                                    byte[] content = cell.Content();
                                    XDocument doc;
                                    if (content.Length == 0)
                                    {

                                        var memory = new MemoryStream();
                                        XmlWriterSettings xws = new XmlWriterSettings();
                                        xws.OmitXmlDeclaration = false;
                                        xws.Indent = true;
                                        xws.Encoding = encoding;

                                        using (XmlWriter xw = XmlWriter.Create(memory, xws))
                                        {
                                            doc =
                                                new XDocument(
                                                    new XDeclaration("1.0", encoding.ToString(), "yes"),
                                                    new XElement(name)
                                                );
                                            doc.WriteTo(xw);
                                        }

                                        memory.Seek(0, SeekOrigin.Begin);
                                        cell.Update(new InputOf(memory));
                                        memory.Seek(0, SeekOrigin.Begin);
                                        content = new BytesOf(new InputOf(memory)).AsBytes();
                                    }
                                    else
                                    {
                                        doc =
                                            XDocument.Parse(
                                                new TextOf(
                                                    new InputOf(content),
                                                    encoding
                                                ).AsString()
                                            );
                                    }
                                    return doc;
                                }
                            }),
                            xnode =>
                            {
                                using (cell)
                                {
                                    cell.Update(
                                        new InputOf(
                                            encoding.GetBytes(xnode.ToString())
                                        )
                                    );
                                }
                            }
                        );
                }
            })
        )
        { }
    }
}
