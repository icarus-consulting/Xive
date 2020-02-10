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

using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument.Test
{
    public sealed class MemorizedXocumentTests
    {
        [Theory]
        [InlineData("A")]
        [InlineData("B")]
        public void ReadsContent(string expected)
        {
            var mem = new RamMemories();
            mem.XML()
                .Update(
                    "xml-xocks",
                    new XMLCursor(
                        new InputOf("<root><item>A</item><item>B</item></root>")
                    ).AsNode()
                );

            Assert.Contains(
                expected,
                new MemorizedXocument("xml-xocks", mem).Values("//item/text()")
            );
        }

        [Fact]
        public void ModifiesContent()
        {
            var mem = new RamMemories();
            mem.XML()
                .Update(
                    "xml-xocks",
                    new XMLCursor(
                        new InputOf("<root><item>A</item></root>")
                    ).AsNode()
                );

            var xoc = new MemorizedXocument("xml-xocks", mem);

            xoc.Modify(
                    new Directives()
                        .Xpath("//item")
                        .Set("B")
                );

            Assert.Contains(
                "B",
                xoc.Values("//item/text()")
            );
        }

        [Fact]
        public void FindsNodes()
        {
            var mem = new RamMemories();
            mem.XML()
                .Update(
                    "xml-xocks",
                    new XMLCursor(
                        new InputOf("<root><item>A</item><item>B</item></root>")
                    ).AsNode()
                );

            var xoc = new MemorizedXocument("xml-xocks", mem);

            Assert.Equal(
                1,
                xoc.Nodes("//item[text() = 'A']").Count
            );
        }

        [Fact]
        public void HasNodeContent()
        {
            var mem = new RamMemories();
            mem.XML()
                .Update(
                    "xml-xocks",
                    new XMLCursor(
                        new InputOf("<root><item>A</item><item>B</item></root>")
                    ).AsNode()
                );

            var xoc = new MemorizedXocument("xml-xocks", mem);

            Assert.Equal(
                "A",
                xoc.Nodes("//item")[0].Values("text()")[0]
            );
        }
    }
}
