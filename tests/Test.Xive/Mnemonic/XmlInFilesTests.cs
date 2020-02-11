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
using System.Xml.Linq;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Xml;

namespace Xive.Test.Mnemonic
{
    public sealed class XmlInFilesTests
    {
        [Fact]
        public void Memorizes()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new XmlInFiles(temp.Value().FullName);
                mem.Content("childhood/puberty", () => new XDocument(new XElement("root", new XText("1990's"))));

                Assert.Equal(
                    "1990's",
                    new XMLCursor(
                        mem.Content(
                            "childhood/puberty",
                            () => throw new ApplicationException("Assumed to have memory")
                        )
                    ).Values("/root/text()")[0]
                );
            }
        }

        [Fact]
        public void Updates()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new XmlInFiles(temp.Value().FullName);
                mem.Content("childhood", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
                mem.Update("childhood", new XDocument(new XElement("root", new XElement("years", new XText("nothing")))));

                Assert.Equal(
                    "nothing",
                    new XMLCursor(
                        mem.Content(
                            "childhood",
                            () => throw new ApplicationException("Assumed to have memory")
                        )
                    ).Values("/root/years/text()")[0]
                );
            }
        }

        [Fact]
        public void DeliversKnowledge()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new XmlInFiles(temp.Value().FullName);
                mem.Content("childhood", () => new XDocument(new XElement("root", new XText("1980's"))));
                Assert.Contains(
                    "childhood",
                    mem.Knowledge()
                );
            }
        }

        [Fact]
        public void RemovesIfEmpty()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new XmlInFiles(temp.Value().FullName);
                mem.Content("childhood", () => new XDocument(new XElement("root", new XText("1980's"))));
                mem.Update("childhood", new XDocument(new XElement("root")));

                Assert.False(mem.Knows("childhood"));
            }
        }
    }
}
