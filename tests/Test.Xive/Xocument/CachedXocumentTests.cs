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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Xive.Cell;
using Xive.Hive;
using Xive.Test;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;
using Yaapii.Xml;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Xocument.Test
{
    public sealed class CachedXocumentTests
    {
        [Fact]
        public void FillsCacheWithContent()
        {
            var cache = new SimpleMemory();

            new CachedXocument(
                "speedup/buffered.xml",
                new SimpleXocument("buffered"),
                cache
            ).Node();

            Assert.Equal(
                "<buffered />",
                cache.Xml("speedup\\buffered.xml", () => new XElement("not-this")).ToString()
            );
        }

        [Fact]
        public void BlacklistsItems()
        {
            var reads = 0;
            var cache = new BlacklistCache("some/*/is/*/buffered.xml");
            var xoc =
                new CachedXocument(
                    "some/path/where/a/file\\is/placed/buffered.xml",
                    new FkXocument(() =>
                    {
                        reads++;
                        return new XDocument(new XElement("buffered"));
                    }),
                    cache
                );

            xoc.Node();
            xoc.Node();

            Assert.Equal(2, reads);
        }

        [Fact]
        public void ReadsContentFromCache()
        {
            var reads = 0;
            var cache = new SimpleMemory();
            var xoc =
                new CachedXocument(
                    "speedup/buffered.xml",
                    new FkXocument(() =>
                    {
                        reads++;
                        return new XDocument(new XElement("buffered"));
                    }),
                    cache
                );
            xoc.Node();
            xoc.Node();

            Assert.Equal(1, reads);
        }

        [Theory]
        [InlineData("UTF-7")]
        [InlineData("UTF-8")]
        [InlineData("UTF-16")]
        [InlineData("UTF-32")]
        public void WorksWithEncoding(string name)
        {
            var encoding = Encoding.GetEncoding(name);
            var inBytes = encoding.GetBytes("Can or can't I dö prüpär äncöding?");

            var cache = new SimpleMemory();

            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, "encoded.xml")))
            {
                var xoc =
                    new CachedXocument(
                        "chars/encoded.xml",
                        new CellXocument(
                            item,
                            "chars\\encoded.xml"
                        ),
                        cache
                    );

                xoc.Modify(
                    new Directives()
                        .Xpath("encoded")
                        .Set(
                            new TextOf(
                                new InputOf(inBytes),
                                encoding
                            ).AsString()
                        )
                    );

                Assert.Equal(
                    "Can or can't I dö prüpär äncöding?",
                    xoc.Value("/encoded/text()", string.Empty)
                );
            }
        }

        [Fact]
        public void PreventsTypeSwitching()
        {
            var cache = new SimpleMemory();
            var content = new InputOf("add some data so that cache memorizes").Stream();
            var memory = new MemoryStream();
            content.CopyTo(memory);
            memory.Seek(0, SeekOrigin.Begin);
            cache.Binary("cash\\cached.xml", () => memory);
            var xoc =
                new CachedXocument(
                    "cash/cached.xml",
                    new FkXocument(),
                    cache
                );

            Assert.Throws<InvalidOperationException>(
                () => xoc.Value("/irrelevant", String.Empty)
            );
        }

        [Fact]
        public void UpdatesCache()
        {
            var cache = new SimpleMemory();
            var xoc =
                new CachedXocument(
                    "update-me/buffered.xml",
                    new SimpleXocument("buffered"),
                    cache
                );
            xoc.Node();
            xoc.Modify(
                new Directives()
                    .Xpath("/buffered")
                    .Set("10 Minutes")
            );

            Assert.Equal(
                "10 Minutes",
                new XMLCursor(
                    cache.Xml(
                        "update-me\\buffered.xml", 
                        () => new XElement("not-this")
                    )
                ).Values("/buffered/text()")[0]
            );
        }
    }
}
