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
            var cache = new Dictionary<string, XNode>();
            new CachedXocument(
                "buffered.xml",
                new SimpleXocument("buffered"),
                cache,
                new Dictionary<string, MemoryStream>()
            ).Node();

            Assert.Equal(
                "<buffered />",
                cache["buffered.xml"].ToString()
            );
        }

        [Fact]
        public void BlacklistsItems()
        {
            var reads = 0;
            var cache = new Dictionary<string, XNode>();
            var xoc =
                new CachedXocument(
                    "some/path/where/a/file/is/placed/buffered.xml",
                    new FkXocument(() =>
                    {
                        reads++;
                        return new XDocument(new XElement("buffered"));
                    }),
                    cache,
                    new Dictionary<string, MemoryStream>(),
                    new List<string>()
                    {
                        "some/*/is/*/buffered.xml"
                    }
                );

            xoc.Node();
            xoc.Node();

            Assert.Equal(2, reads);
        }

        [Fact]
        public void ReadsContentFromCache()
        {
            var reads = 0;
            var cache = new Dictionary<string, XNode>();
            var xoc =
                new CachedXocument(
                    "buffered.xml",
                    new FkXocument(() =>
                    {
                        reads++;
                        return new XDocument(new XElement("buffered"));
                    }),
                    cache,
                    new Dictionary<string, MemoryStream>()
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

            var binCache = new Dictionary<string, MemoryStream>();
            var xmlCache = new Dictionary<string, XNode>();

            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, "encoded.xml")))
            {
                var xoc =
                    new CachedXocument(
                        "encoded.xml",
                        new CellXocument(
                            item,
                            "encoded.xml"
                        ),
                        xmlCache,
                        binCache
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
            var xmlCache = new Dictionary<string, XNode>();
            var binCache = new Dictionary<string, MemoryStream>();
            binCache.Add("cached.xml", new MemoryStream());
            var xoc =
                new CachedXocument(
                    "cached.xml",
                    new FkXocument(),
                    xmlCache,
                    binCache
                );

            Assert.Throws<InvalidOperationException>(
                () => xoc.Value("/irrelevant", String.Empty)
            );
        }

        [Fact]
        public void UpdatesCache()
        {
            var cache = new Dictionary<string, XNode>();
            var xoc =
                new CachedXocument(
                    "buffered.xml",
                    new SimpleXocument("buffered"),
                    cache,
                    new Dictionary<string, MemoryStream>()
                );
            xoc.Node();
            xoc.Modify(
                new Directives()
                    .Xpath("/buffered")
                    .Set("10 Minutes")
            );

            Assert.Equal(
                "10 Minutes",
                new XMLCursor(cache["buffered.xml"]).Values("/buffered/text()")[0]
            );
        }
    }
}
