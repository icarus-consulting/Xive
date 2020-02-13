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
using System.IO;
using System.Xml.Linq;
using Xive.Cell;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xml;

namespace Xive.Mnemonic.Test
{
    public sealed class CachedMemoriesTests
    {
        [Fact]
        public void CachesData()
        {
            var mem = new RamMemories();
            var cache = new CachedMemories(mem);
            var data = new BytesOf(new InputOf("splashy")).AsBytes();

            cache.Data().Content("cashy", () => data); //read 1
            mem.Data().Update("cashy", new byte[0]);

            Assert.Equal(
                "splashy",
                new TextOf(
                    new InputOf(
                        cache.Data().Content("cashy", () => throw new ApplicationException($"Assumed to have memory"))
                    )
                ).AsString()
            );

        }

        [Fact]
        public void CachesXml()
        {
            var data = (XNode)new XDocument(new XElement("root", new XText("potato")));
            var mem = new RamMemories();
            var cache = new CachedMemories(mem);

            cache.XML().Content("cashy", () => data); //read 1
            mem.XML().Update("cashy", (XNode)new XDocument(new XElement("root", new XText(""))));

            Assert.Contains(
                "potato",
                new XMLCursor(
                    cache.XML().Content("cashy", () => throw new ApplicationException($"Assumed to have memory"))
                ).Values("/root/text()")
            );
        }

        [Fact]
        public void BlacklistsItems()
        {
            var mem = new RamMemories();
            var cache = new CachedMemories(mem, "a/*/blacklisted/*");
            var cell =
                    new MemorizedCell(
                        "a/file\\which/is\\blacklisted/data.dat",
                        cache
                    );

            cell.Content();
            mem.Data().Update("a/file\\which/is\\blacklisted/data.dat", new byte[128]);

            Assert.False(cache.Data().Knows("a/file\\which/is\\blacklisted/data.dat"));
        }

        [Fact]
        public void DoesNotCacheOversized()
        {
            var mem = new RamMemories();
            var cache = new CachedMemories(mem, 4);
            var cell =
                new MemorizedCell(
                    "a/file/which/is/oversized",
                    cache
                );
            cell.Update(new InputOf(new byte[128]));
            cell.Content();
            mem.Data().Update("a/file/which/is/oversized", new byte[0]);

            Assert.True(cache.Data().Content("a/file/which/is/oversized", () => new byte[0]).Length == 0);
        }
    }
}
