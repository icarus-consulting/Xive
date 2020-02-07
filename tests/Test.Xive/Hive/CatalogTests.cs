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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Xive.Comb;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Xambly;

namespace Xive.Hive.Test
{
    public sealed class SimpleCatalogTests
    {
        [Theory]
        [InlineData("123")]
        [InlineData("456")]
        public void AddsEntry(string id)
        {
            var catalog =
                new SimpleCatalog("my-scope",
                    new RamComb("hq")
                );

            catalog.Create("123");
            catalog.Create("456");

            Assert.Contains(
                id,
                catalog.List("'*'")
            );
        }

        [Fact]
        public void RemovesEntry()
        {
            var catalog =
                new SimpleCatalog("my-scope",
                    new RamComb("hq")
                );

            catalog.Update("123", new Directives());
            catalog.Remove("123");

            Assert.DoesNotContain(
                "123",
                catalog.List("*")
            );
        }

        [Fact]
        public void UpdatesEntry()
        {
            var catalog =
                new SimpleCatalog("my-scope",
                    new RamComb("hq")
                );

            catalog.Create("123");
            catalog.Update(
                "123",
                new Directives()
                    .Add("todo")
                    .Set("code some stuff")
            );

            Assert.Contains(
                "123",
                catalog.List("todo[text()='code some stuff']")
            );
        }

        [Fact]
        public void WorksParallelWithDirectAccessToCatalog()
        {
            using (var dir = new TempDirectory())
            {
                var locks = new ConcurrentDictionary<string, Mutex>();
                var valve = new ProcessSyncValve(locks);
                var hive =
                    new SyncHive(
                       new FileHive(
                           "machine",
                           dir.Value().FullName
                       ),
                       valve
                    );
                try
                {
                    Parallel.For(0, Environment.ProcessorCount << 4, i =>
                    {
                        using (var xoc = hive.HQ().Xocument("catalog.xml"))
                        {
                            xoc.Modify(
                                new Directives().Xpath("/catalog")
                                .Add("machine").Attr("id", $"123{i.ToString()}").Set("someContent")
                            );
                        }
                        Assert.NotEmpty(hive.Combs("'*'"));
                    });
                }
                catch (Exception ex)
                {
                    var keys = locks.Keys;
                    throw;
                }
            }
        }

        [Fact]
        public void WorksParallelWithCachedHive()
        {
            using (var dir = new TempDirectory())
            {
                var hive =
                    new SyncHive(
                        new CachedHive(
                           new FileHive(
                               "machine",
                               dir.Value().FullName
                           )
                        )
                    );
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    using (var xoc = hive.HQ().Xocument("catalog.xml"))
                    {
                        xoc.Modify(
                            new Directives().Xpath("/catalog")
                            .Add("machine").Attr("id", $"123{i.ToString()}").Set("someContent")
                        );
                    }
                    var comb = new FirstOf<IHoneyComb>(hive.Combs($"@id='123{i.ToString()}'")).Value();
                    using (var xoc = comb.Xocument("index.xml"))
                    {
                        xoc.Modify(new Directives().Xpath("/index").Add("node").Set("content"));
                    }
                });
            }
        }
    }
}
