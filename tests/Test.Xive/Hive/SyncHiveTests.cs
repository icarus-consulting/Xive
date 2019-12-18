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
using System.Diagnostics;
using System.Threading.Tasks;
using Test.Yaapii.Xive;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public class SyncHiveTest
    {
        [Fact]
        public void CreatesCatalogInParallel()
        {
            var valve = new ProcessSyncValve();
            var hive = new SyncHive(new RamHive("testRamHive"), valve);

            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                var catalog = new SyncCatalog(hive, valve);
                catalog.Create("123");
            });
        }

        [Fact]
        public void DeliversCombsInParallel()
        {
            var valve = new ProcessSyncValve();
            var hive = new SyncHive(new RamHive("product"), valve);
            new SyncCatalog(hive, valve).Create("2CV");

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.Combs("@id='2CV'");
            });

        }

        [Fact]
        public void DoesNotBlockItself()
        {
            var valve = new ProcessSyncValve();
            var hive = new SyncHive(new RamHive("product"), valve);
            new SyncCatalog(hive, valve).Create("2CV");
            using (var xoc = hive.Shifted("machine").HQ().Xocument("catalog.xml"))
            {
                for (int i = 0; i < 256; i++)
                {
                    xoc.Modify(
                        new Directives()
                            .Xpath("/catalog")
                            .Add("machine")
                            .Attr("id", i.ToString())
                            .Add("name").Set("mech")
                        );
                }
            }

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                using (var xoc = hive.Shifted("machine").HQ().Xocument("catalog.xml"))
                {
                    var name = xoc.Value($"/catalog/machine[id='{i}']", "");
                    xoc.Modify(new Directives().Xpath("//name").Set(Guid.NewGuid()));
                    using (var xoc2 = hive.Shifted("machine").HQ().Xocument("catalog.xml"))
                    {
                        var name2 = xoc2.Value($"/catalog/machine[id='{i}']", "");
                        xoc2.Modify(new Directives().Xpath("//name").Set(Guid.NewGuid()));
                        using (var xoc3 = hive.Shifted("machine").HQ().Xocument("catalog.xml"))
                        {
                            var name3 = xoc3.Value($"/catalog/machine[id='{i}']", "");
                            xoc3.Modify(new Directives().Xpath("//name").Set(Guid.NewGuid()));
                        }
                    }
                }
                hive.Combs("@id='2CV'");
            });

        }

        [Fact]
        public void Shifts()
        {
            var hive =
                new SyncHive(
                    new RamHive("person")
                );

            Assert.Equal(
                "left",
                hive.Shifted("left").Scope()
            );
        }

        [Fact]
        public void DeliversHQInParallelAfterShift()
        {
            var valve = new ProcessSyncValve();
            var hive =
                new SyncHive(
                    new RamHive("person"),
                    valve
                ).Shifted("still-parallel");

            var first = true;
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                if (!first)
                {
                    new SyncCatalog(hive, valve).Remove("X");
                    first = false;
                }
                new SyncCatalog(hive, valve).Create("X");
            });
        }

        [Fact]
        public void DeliversHQInParallel()
        {
            var hive =
                new SyncHive(
                    new RamHive("person")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                using (var xoc = hive.HQ().Xocument("test"))
                {
                    xoc.Node().ToString();
                }
            });
        }

        [Fact]
        public void DeliversNameInParallel()
        {
            using (var dir = new TempDirectory())
            {
                var hive = 
                    new SyncHive(
                        new FileHive("product", dir.Value().FullName)
                    );
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    hive.Scope();
                });
            }
        }

        [Fact]
        public void WorksWithFileHive()
        {
            using (var dir = new TempDirectory())
            {
                var valve = new ProcessSyncValve();
                var hive = 
                    new SyncHive(
                        new FileHive("product", dir.Value().FullName)
                    );
                var catalog = new SyncCatalog(hive, valve);
                catalog.Create("2CV");
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    hive.Combs("@id='2CV'");
                    hive.Scope();
                    hive.HQ();
                });
            }
        }

        [Fact]
        public void WorksParallel()
        {
            var valve = new ProcessSyncValve();
            var hive = new SyncHive(new RamHive(), valve);
            var comb = "Dr.Robotic";
            new ParallelFunc(() =>
                {
                    var id = "Item_" + new Random().Next(1, 5);
                    var content = Guid.NewGuid().ToString();

                    new SyncCatalog(
                        hive,
                        new SimpleCatalog(hive), 
                        valve
                    ).Create("Dr.Robotic");

                    using (var item =
                        new FirstOf<IHoneyComb>(
                            hive.Combs($"@id='{comb}'")
                        ).Value().Cell(id)
                    )
                    {
                        item.Update(new InputOf(content));
                        Assert.Equal(content, new TextOf(item.Content()).AsString());
                    }
                    return true;
                },
                Environment.ProcessorCount << 4,
                10000
            ).Invoke();
        }

        [Fact]
        public void WorksParallelWithFileHive()
        {
            using (var dir = new TempDirectory())
            {
                var valve = new ProcessSyncValve();
                var hive =
                    new SyncHive(
                        new CachedHive(
                            new FileHive(dir.Value().FullName)
                        ),
                        valve
                    );

                var machine = "Dr.Robotic";
                new ParallelFunc(() =>
                {
                    var id = "Item_" + new Random().Next(1, 5);
                    try
                    {
                        new SyncCatalog(
                            hive.Shifted("to-the-left"),
                            new SimpleCatalog(hive.Shifted("to-the-left")),
                            valve
                        ).Create("Dr.Robotic");
                    }
                    catch (InvalidOperationException)
                    {
                        //ignored with intention
                    }

                    using (var cell =
                        new FirstOf<IHoneyComb>(
                            hive.Shifted("to-the-left").Combs($"@id='{machine}'")
                        ).Value().Cell(id)
                    )
                    {
                        var content = Guid.NewGuid().ToString();
                        cell.Update(new InputOf(content));
                        Assert.Equal(content, new TextOf(cell.Content()).AsString());
                    }
                    return true;
                },
                1, //Environment.ProcessorCount << 4,
                10000
            ).Invoke();
            }
        }

        [Fact]
        public void WorksParallelWithRamHive()
        {
            var valve = new ProcessSyncValve();
            var hive =
                new SyncHive(
                    new CachedHive(
                        new RamHive()
                    ),
                    valve
                );

            var machine = "Dr.Robotic";
            new ParallelFunc(() =>
                {
                    var id = "Item_" + new Random().Next(1, 5);
                    try
                    {
                        new SimpleCatalog(
                            hive.Shifted("to-the-left")
                        ).Create(machine);
                    }
                    catch (InvalidOperationException)
                    {
                        //ignored with intention
                    }

                    using (var cell =
                        new FirstOf<IHoneyComb>(
                            hive.Shifted("to-the-left").Combs($"@id='{machine}'")
                        ).Value().Cell(id)
                    )
                    {
                        var content = Guid.NewGuid().ToString();
                        cell.Update(new InputOf(content));
                        Assert.Equal(content, new TextOf(cell.Content()).AsString());
                    }
                    return true;
                },
                Environment.ProcessorCount << 4,
                10000
            ).Invoke();
        }
    }
}