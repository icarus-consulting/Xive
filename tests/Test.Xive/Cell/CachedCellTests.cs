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
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Cell.Test
{
    public sealed class CachedCellTests
    {
        [Fact]
        public void ReadsOnce()
        {
            var cache = new Dictionary<string, MemoryStream>();
            int reads = 0;
            var cell =
                new CachedCell(
                    new FkCell(
                        content => { },
                        () =>
                        {
                            reads++;
                            return new byte[0];
                        }
                    ),
                    "cached",
                    cache,
                    new Dictionary<string, XNode>()
                );

            cell.Content();
            cell.Content();

            Assert.Equal(1, reads);
        }

        [Fact]
        public void FillsCache()
        {
            var cache = new Dictionary<string, MemoryStream>();
            new CachedCell(
                new FkCell(
                    content => { },
                    () =>
                        new BytesOf(
                            new InputOf("Some content")
                        ).AsBytes()
                ),
                "cached",
                cache,
                new Dictionary<string, XNode>()
            ).Content();

            Assert.Equal(
                "Some content",
                new TextOf(
                    new InputOf(cache["cached"])
                ).AsString()
            );
        }

        [Fact]
        public void UpdatesCache()
        {
            var cache = new Dictionary<string, MemoryStream>();
            var cell =
                new CachedCell(
                    new FkCell(
                        content => { },
                        () => new BytesOf(new InputOf("Old content")).AsBytes()
                    ),
                    "cached",
                    cache,
                    new Dictionary<string, XNode>()
                );
            cell.Content();
            cell.Update(new InputOf("New content"));

            Assert.Equal(
                "New content",
                new TextOf(
                    new InputOf(cache["cached"])
                ).AsString()
            );
        }

        [Fact]
        public void PreventsTypeSwitching()
        {
            var binCache = new Dictionary<string, MemoryStream>();
            var xmlCache = new Dictionary<string, XNode>();
            xmlCache.Add("cached", new XElement("irrelevant"));

            var binCell =
                new CachedCell(
                    new FkCell(),
                    "cached",
                    binCache,
                    xmlCache
                );

            Assert.Throws<InvalidOperationException>(
                () => binCell.Content()
            );
        }

        [Fact]
        public void ReadSkipsCacheWhenOversized()
        {
            var cache = new Dictionary<string, MemoryStream>();
            var cell =
                new CachedCell(
                    new FkCell(
                        (update) => { },
                        () => new byte[128]
                    ),
                    "cached",
                    cache,
                    new Dictionary<string, XNode>(),
                    64
                );
            cell.Content();

            Assert.DoesNotContain(
                "cached",
                cache.Keys
            );
        }

        [Fact]
        public void UpdateSkipsCacheWhenOversized()
        {
            var cache = new Dictionary<string, MemoryStream>();
            var cell =
                new CachedCell(
                    new FkCell(),
                    "cached",
                    cache,
                    new Dictionary<string, XNode>(),
                    64
                );
            cell.Update(
                new InputOf(new byte[128])
            );

            Assert.DoesNotContain(
                "cached",
                cache.Keys
            );
        }

        [Fact]
        public void UnderMaxAndThenOverMaxWorks()
        {
            var cache = new Dictionary<string, MemoryStream>();
            var cell =
                new CachedCell(
                    new RamCell(),
                    "cached",
                    cache,
                    new Dictionary<string, XNode>(),
                    3
                );
            cell.Update(
                new InputOf(new byte[0])
            );

            cell.Update(
                new InputOf("More than 3 characters")
            );

            Assert.Equal(
                "More than 3 characters",
                new TextOf(cell.Content()).AsString()
            );
        }
    }
}
