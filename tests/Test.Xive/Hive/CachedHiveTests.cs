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
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Xive.Comb;
using Xive.Test;
using Xive.Xocument;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public sealed class CachedHiveTests
    {
        [Fact]
        public void ReadsBinaryOnce()
        {
            var cache = new SimpleCache();
            int reads = 0;
            var hive =
                new CachedHive(
                    new SimpleHive("phonebook",
                        combName =>
                        new SimpleComb(
                            "my-comb",
                            cellname =>
                                new FkCell(
                                    content => { },
                                    () =>
                                    {
                                        reads++;
                                        return new byte[0];
                                    }
                                ),
                            (x, c) => new CachedXocument(x, new SimpleXocument("catalog"), cache)
                        )
                    ),
                    cache
                );

            new SimpleCatalog(hive).Create("123");

            var cell =
                new FirstOf<IHoneyComb>(
                    hive.Combs("@id='123'")
                )
                .Value()
                .Cell("adalbert");

            cell.Content();
            cell.Content();

            Assert.Equal(1, reads);
        }

        [Fact]
        public void ConsidersMaxBytes()
        {
            var cache = new LimitedCache(0, new SimpleCache());
            int reads = 0;
            var hive =
                new CachedHive(
                    new SimpleHive("phonebook",
                        combName =>
                        new SimpleComb(
                            "my-comb",
                            cellname =>
                                new FkCell(
                                    content => { },
                                    () =>
                                    {
                                        reads++;
                                        return new byte[20];
                                    }
                                ),
                            (x, c) => new CachedXocument(x, new SimpleXocument("catalog"), cache)
                        )
                    ),
                    cache
                );

            new SimpleCatalog(hive).Create("123");

            var cell =
                new FirstOf<IHoneyComb>(
                    hive.Combs("@id='123'")
                )
                .Value()
                .Cell("adalbert");

            cell.Content();
            cell.Content();

            Assert.Equal(2, reads);
        }

        [Fact]
        public void ConsidersMaxBytesOnShifted()
        {
            var cache = new LimitedCache(0, new SimpleCache());
            int reads = 0;
            var hive =
                new CachedHive(
                    new SimpleHive("phonebook",
                        combName =>
                        new SimpleComb(
                            "my-comb",
                            cellname =>
                                new FkCell(
                                    content => { },
                                    () =>
                                    {
                                        reads++;
                                        return new byte[20];
                                    }
                                ),
                            (x, c) => new CachedXocument(x, new SimpleXocument("catalog"), cache)
                        )
                    ),
                    cache
                );

            new SimpleCatalog(hive.Shifted("A")).Create("123");

            var cell =
                new FirstOf<IHoneyComb>(
                    hive.Shifted("A").Combs("@id='123'")
                )
                .Value()
                .Cell("adalbert");

            cell.Content();
            cell.Content();

            Assert.Equal(2, reads);
        }

        [Fact]
        public void ConsidersMaxBytesOnHQ()
        {
            var cache = new LimitedCache(0, new SimpleCache());
            int reads = 0;
            var hive =
                new CachedHive(
                    new SimpleHive("phonebook",
                        combName =>
                        new SimpleComb(
                            "my-comb",
                            cellname =>
                                new FkCell(
                                    content => { },
                                    () =>
                                    {
                                        reads++;
                                        return new byte[20];
                                    }
                                ),
                            (x, c) => new CachedXocument(x, new SimpleXocument("catalog"), cache)
                        )
                    ),
                    cache
                );

            var cell = hive.HQ().Cell("adalbert");

            cell.Content();
            cell.Content();

            Assert.Equal(2, reads);
        }

        [Fact]
        public void BlackListsBinaries()
        {
            var cache = new BlacklistCache("*dal*rt*");
            int reads = 0;
            var hive =
                new CachedHive(
                    new SimpleHive("phonebook",
                        combName =>
                        new SimpleComb(
                            "my-comb",
                            cellname =>
                                new FkCell(
                                    content => { },
                                    () =>
                                    {
                                        reads++;
                                        return new byte[0];
                                    }
                                ),
                            (x, c) => new CachedXocument(x, new SimpleXocument("catalog"), cache)
                        )
                    ),
                    cache
                );

            new SimpleCatalog(hive).Create("123");

            var cell =
                new FirstOf<IHoneyComb>(
                    hive.Combs("@id='123'")
                )
                .Value()
                .Cell("adalbert");

            cell.Content();
            cell.Content();

            Assert.Equal(2, reads);
        }

        [Fact]
        public void BlackListsShiftedBinaries()
        {
            var cache = new BlacklistCache("*dal*rt*");
            int reads = 0;
            var hive =
                new CachedHive(
                    new SimpleHive("phonebook",
                        combName =>
                        new SimpleComb(
                            "my-comb",
                            cellname =>
                                new FkCell(
                                    content => { },
                                    () =>
                                    {
                                        reads++;
                                        return new byte[0];
                                    }
                                ),
                            (x, c) => new CachedXocument(x, new SimpleXocument("catalog"), cache)
                        )
                    ),
                    cache
                ).Shifted("hello");

            new SimpleCatalog(hive).Create("123");

            var cell =
                new FirstOf<IHoneyComb>(
                    hive.Combs("@id='123'")
                )
                .Value()
                .Cell("adalbert");

            cell.Content();
            cell.Content();

            Assert.Equal(2, reads);
        }

        [Fact]
        public void ShiftingRamHiveIncludesScope()
        {
            var cache = new SimpleCache();

            var hive =
                new CachedHive(
                    new RamHive(),
                    cache
                );

            new SimpleCatalog(hive.Shifted("A")).Create("something");
            new SimpleCatalog(hive.Shifted("B")).Create("another thing");

            Assert.Contains(
                "another thing",
                cache.Xml(
                    @"B/HQ/catalog.xml",
                    () => new XElement("not-this")).ToString()
                );
        }

        [Fact]
        public void ShiftingFileHiveIncludesScope()
        {
            using (var dir = new TempDirectory())
            {
                var cache = new SimpleCache();

                var hive =
                    new CachedHive(
                        new FileHive(dir.Value().FullName),
                        cache
                    );

                new SimpleCatalog(hive.Shifted("A")).Create("something");
                new SimpleCatalog(hive.Shifted("B")).Create("another thing");

                Assert.Contains(
                    "another thing",
                    cache.Xml(@"B/HQ/catalog.xml", () => new XElement("not-this")).ToString()
                );
            }
        }

        [Fact]
        public void ShiftingDoesNotOverrideCells()
        {
            using (var dir = new TempDirectory())
            {
                var cache = new SimpleCache();

                var hive =
                    new CachedHive(
                        new FileHive(dir.Value().FullName),
                        cache
                    );

                new SimpleCatalog(hive.Shifted("scope-1")).Create("ambiguous-item");
                new SimpleCatalog(hive.Shifted("scope-2")).Create("ambiguous-item");

                using (
                    var cell =
                        new FirstOf<IHoneyComb>(
                            hive.Shifted("scope-1").Combs("@id='ambiguous-item'")
                        ).Value().Cell("ambiguous.xml")
                )
                {
                    cell.Update(new InputOf("here is some data"));
                }

                using (
                    var cell =
                        new FirstOf<IHoneyComb>(
                            hive.Shifted("scope-2").Combs("@id='ambiguous-item'")
                        ).Value().Cell("ambiguous.xml")
                )
                {
                    Assert.Empty(cell.Content());
                }
            }
        }

        [Fact]
        public void ShiftingDoesNotOverrideXocuments()
        {
            using (var dir = new TempDirectory())
            {
                var cache = new SimpleCache();

                var hive =
                    new CachedHive(
                        new FileHive(dir.Value().FullName),
                        cache
                    );

                new SimpleCatalog(hive.Shifted("scope-1")).Create("ambiguous-item");
                new SimpleCatalog(hive.Shifted("scope-2")).Create("ambiguous-item");

                using (
                    var xoc =
                        new FirstOf<IHoneyComb>(
                            hive.Shifted("scope-1").Combs("@id='ambiguous-item'")
                        ).Value().Xocument("ambiguous.xml")
                )
                {
                    xoc.Modify(new Directives().Xpath("/ambiguous").Add("entry"));
                }

                using (
                    var xoc =
                        new FirstOf<IHoneyComb>(
                            hive.Shifted("scope-2").Combs("@id='ambiguous-item'")
                        ).Value().Xocument("ambiguous.xml")
                )
                {
                    Assert.Empty(xoc.Nodes("/ambiguous/entry"));
                }
            }
        }

        [Fact]
        public void WorksParallelWithRamHive()
        {
            var hive =
                new SyncHive(
                    new CachedHive(
                        new RamHive()
                    )
                );
            var machine = "Dr.Robotic";
            new SimpleCatalog(hive).Create(machine);
            for (var i = 0; i < 2; i++)
            {
                var id = "item";

                using (var cell =
                    new FirstOf<IHoneyComb>(
                        hive.Combs($"@id='{machine}'")
                    ).Value().Cell(id)
                )
                {
                    var content = Guid.NewGuid().ToString();
                    cell.Update(new InputOf(content));
                    Assert.Equal(content, new TextOf(cell.Content()).AsString());
                }
            }
        }
    }
}