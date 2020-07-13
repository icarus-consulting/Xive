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
using Xive.Cell;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xml;

namespace Xive.Mnemonic.Test
{
    public sealed class CachedMnemonicTests
    {
        [Fact]
        public void CachesBytesOnRead()
        {
            var mem = new RamMnemonic();
            var cache = new CachedMnemonic(mem);
            var data = new BytesOf(new InputOf("splashy")).AsBytes();

            cache.Contents().Bytes("cashy", () => data); //read 1
            mem.Contents().UpdateBytes("cashy", new byte[0]);

            Assert.Equal(
                "splashy",
                new TextOf(
                    new InputOf(
                        cache.Contents()
                            .Bytes("cashy", () => throw new ApplicationException($"Assumed to have memory"))
                    )
                ).AsString()
            );

        }

        [Fact]
        public void CachesXmlOnRead()
        {
            var xml = (XNode)new XDocument(new XElement("root", new XText("potato")));
            var mem = new RamMnemonic();
            var cache = new CachedMnemonic(mem);

            cache.Contents().Xml("cashy", () => xml); //read 1
            mem.Contents().UpdateXml("cashy", (XNode)new XDocument(new XElement("root", new XText(""))));

            Assert.Contains(
                "potato",
                new XMLCursor(
                    cache.Contents()
                        .Xml("cashy", () => throw new ApplicationException($"Assumed to have memory"))
                ).Values("/root/text()")
            );
        }

        [Fact]
        public void IgnoresItems()
        {
            var mem = new RamMnemonic();
            var cache = new CachedMnemonic(mem, "a/*/blacklisted/*");
            var cell =
                    new MemorizedCell(
                        "a/file\\which/is\\blacklisted/data.dat",
                        cache
                    );

            cell.Content();
            mem.Contents()
                .UpdateBytes("a/file\\which/is\\blacklisted/data.dat", new byte[128]);

            Assert.False(cache.Contents().Knowledge().Contains("a/file\\which/is\\blacklisted/data.dat"));
        }

        [Fact]
        public void DoesNotCacheOversized()
        {
            var mem = new RamMnemonic();
            var cache = new CachedMnemonic(mem, 4);
            var cell =
                new MemorizedCell(
                    "a/file/which/is/oversized",
                    cache
                );
            cell.Update(new InputOf(new byte[128]));
            cell.Content();
            mem.Contents().UpdateBytes("a/file/which/is/oversized", new byte[0]);

            Assert.True(cache.Contents().Bytes("a/file/which/is/oversized", () => new byte[0]).Length == 0);
        }

        [Fact]
        public void RemovesXml()
        {
            var mem = new RamMnemonic();
            var cache = new CachedMnemonic(mem);
            var data = new BytesOf(new InputOf("splashy")).AsBytes();

            cache.Contents().UpdateXml("splashy.xml", new XDocument(new XElement("splashy")));
            cache.Contents().UpdateXml("splashy.xml", new XDocument());

            Assert.Equal(
                "<root />",
                cache.Contents().Xml("splashy.xml", () => new XDocument(new XElement("root"))).ToString()
            );
        }
    }
}
