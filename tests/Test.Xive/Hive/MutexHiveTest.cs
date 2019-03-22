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
using System.Threading.Tasks;
using Test.Yaapii.Xive;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public class MutexHiveTest
    {
        [Fact]
        public void CreatesCatalogInParallel()
        {
            var hive = new MutexHive(new RamHive("testRamHive"));

            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                var catalog = new MutexCatalog(hive);
                catalog.Create("123");
            });
        }

        [Fact]
        public void DeliversCombsInParallel()
        {
            var hive = new MutexHive(new RamHive("product"));
            new MutexCatalog(hive).Create("2CV");

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.Combs("@id='2CV'");
            });

        }

        [Fact]
        public void Shifts()
        {
            var hive =
                new MutexHive(
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
            var hive =
                new MutexHive(
                    new RamHive("person")
                ).Shifted("still-parallel");

            var first = true;
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                if (!first)
                {
                    new MutexCatalog(hive).Remove("X");
                    first = false;
                }
                new MutexCatalog(hive).Create("X");
            });
        }

        [Fact]
        public void DeliversHQInParallel()
        {
            var hive =
                new MutexHive(
                    new RamHive("person")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.HQ();
            });
        }

        [Fact]
        public void DeliversNameInParallel()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new MutexHive(new FileHive("product", dir.Value().FullName));
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
                var hive = new MutexHive(new FileHive("product", dir.Value().FullName));
                var catalog = new MutexCatalog(hive);
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
            var hive = new MutexHive(new RamHive());
            var comb = "Dr.Robotic";
            new ParallelFunc(() =>
                {
                    var id = "Item_" + new Random().Next(1, 5);
                    var content = Guid.NewGuid().ToString();

                    new MutexCatalog(hive).Create("Dr.Robotic");

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
                var hive =
                    new MutexHive(
                        new FileHive(dir.Value().FullName)
                    );

                var machine = "Dr.Robotic";
                new ParallelFunc(() =>
                {
                    var id = "Item_" + new Random().Next(1, 5);
                    try
                    {
                        new MutexCatalog(
                            hive.Shifted("to-the-left")
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
                Environment.ProcessorCount << 4,
                10000
            ).Invoke();
            }
        }

        [Fact]
        public void WorksParallelWithRamHive()
        {
            var hive =
                new MutexHive(
                    new CachedHive(
                        new RamHive()
                    )
                );

            var machine = "Dr.Robotic";
            new ParallelFunc(() =>
                {
                    var id = "Item_" + new Random().Next(1, 5);
                    try
                    {
                        new MutexCatalog(
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