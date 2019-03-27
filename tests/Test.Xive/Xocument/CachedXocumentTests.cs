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
using System.Xml.Linq;
using Xive.Test;
using Xunit;
using Yaapii.Xambly;
using Yaapii.Xml;

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
