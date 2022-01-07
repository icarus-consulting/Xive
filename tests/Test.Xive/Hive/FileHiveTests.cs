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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public sealed class FileHiveTests
    {
        [Fact]
        public void DeliversHQWithBackSlashes()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Equal(
                    $"product/hq",
                    new FileHive(dir.Value().FullName, "product")
                        .HQ()
                        .Name()
                );
            }
        }

        [Fact]
        public void DeliversHQWithForwardSlashes()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Equal(
                    $"product/hq",
                    new FileHive(
                        new Normalized(
                            dir.Value().FullName
                        ).AsString(),
                        "product"
                    )
                    .HQ()
                    .Name()
                );
            }
        }

        [Fact]
        public void FindsComb()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");
                hive.Catalog().Add("2CV");
                Assert.NotEmpty(
                    hive.Catalog().List()
                );
            }
        }

        [Fact]
        public void ShiftsHQ()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "cockpit");
                hive.Catalog().Add("log");

                var shifted = hive.Shifted("factory");
                shifted.Catalog().Add("booyaa");

                var shiftedAgain = shifted.Shifted("cockpit");

                Assert.Contains("log", shiftedAgain.Catalog().List()[0].Name());
            }
        }

        [Fact]
        public void PrependsScopeToCombName()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName, "cockpit");
                var shifted = hive.Shifted("prepend-this");
                shifted.Catalog().Add("an-entry");

                Assert.StartsWith("prepend-this",
                    shifted.Comb("an-entry").Name()
                );
            }
        }

        [Fact]
        public void ShiftCreatesSubDir()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName, "cars");
                hive = hive.Shifted("product");
                hive.Catalog().Add("2CV");

                using (var cell =
                    hive.Comb("2CV").Cell("data")
                )
                {
                    cell.Update(new InputOf("bytes over bytes here..."));
                    Assert.True(
                        Directory.Exists(
                            new Normalized(
                                Path.Combine(
                                    dir.Value().FullName,
                                    "product",
                                    "2CV"
                                )
                            ).AsString()
                        )
                    );
                }
            }
        }

        [Fact]
        public void ShiftCreatesHQ()
        {
            using (var dir = new TempDirectory())
            {
                new FileHive(dir.Value().FullName, "product").Catalog().Add("2CV");
                Assert.True(
                    Directory.Exists(
                        new Normalized(
                            Path.Combine(
                                dir.Value().FullName,
                                "product",
                                "hq"
                            )
                        ).AsString()
                    )
                );
            }
        }

        [Fact]
        public void ShiftsScope()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName, "product");
                hive.Catalog().Add("2CV");
                hive = hive.Shifted("machine");
                hive.Catalog().Add("DrRobotic");
                Assert.Equal(
                    1,
                    hive.Catalog().List().Count
                );
            }
        }

        [Fact]
        public void CreatesHiveFolder()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");
                hive.Catalog().Add("2CV");

                using (var cell = hive.Comb("2CV").Cell("Some-testing-item"))
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                    var productDir = Path.Combine(dir.Value().FullName, "product");
                    Assert.True(
                        Directory.Exists(productDir),
                        $"Directory '{productDir}' doesn't exist"
                    );
                }
            }
        }

        [Fact]
        public void CreatesCombFolder()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");
                using (var cell =
                    hive.Comb("2CV", true).Cell("Some-testing-item")
                )
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                    var combDir = Path.Combine(dir.Value().FullName, "product", "2CV");
                    Assert.True(
                        Directory.Exists(
                            new Normalized(
                                combDir
                            ).AsString()
                        )
                        ,
                        $"Directory '{combDir}' doesn't exist"
                    );
                }
            }
        }

        [Fact]
        public void RemembersCombCell()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");
                using (var cell = hive.Comb("2CV").Cell("Some-testing-item")
                )
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                }

                using (var cell = hive.Comb("2CV").Cell("Some-testing-item")
                )
                {
                    Assert.Equal(
                        "I am a very cool testdata string",
                        new TextOf(
                            cell.Content()
                        ).AsString()
                    );
                }
            }
        }

        [Fact]
        public void DeliversScope()
        {
            Assert.Equal("the name", new FileHive("the root", "the name").Scope());
        }

        [Fact]
        public void AddsInParallel()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");

                Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
                {
                    hive.Catalog().Add("supercar");
                });

                Assert.Equal(1, hive.Catalog().List().Count);
            }
        }

        [Fact]
        public void WritesPropsInParallel()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");

                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    hive.Comb("2CV").Props().Refined("looping", "louie");
                });

                Assert.Equal("louie", hive.Comb("2CV").Props().Value("looping"));
            }
        }

        [Fact]
        public void WritesPropsWhenShifted()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");

                Parallel.For(0, 256, (i) =>
                {
                    var id = $"mech-{i}";
                    hive.Shifted("machine").Catalog().Add(id);
                    hive.Shifted("machine")
                        .Comb(id)
                        .Props()
                        .Refined("checksum", id);
                });

                Parallel.ForEach(hive.Shifted("machine").Catalog().List(), comb =>
                {
                    Assert.Equal(comb.Name(), $"machine/{comb.Props().Value("checksum")}");
                });
            }
        }

        [Fact]
        public void WorksAsyncWithCache()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive =
                    new MemorizedHive(
                        "product",
                        new CachedMnemonic(
                            new FileMnemonic(dir.Value().FullName)
                        )
                    ).Shifted("machine");

                long elapsed = 0;

                Parallel.For(0, 256, (i) =>
                {
                    var id = $"mech-{i}";
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    hive.Catalog().Add(id);
                    stopWatch.Stop();
                    elapsed += stopWatch.ElapsedMilliseconds;
                });

                Debug.WriteLine("Creation: " + elapsed);
                elapsed = 0;

                Parallel.For(0, 256, (i) =>
                {
                    var id = $"mech-{i}";
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var prop = hive.Comb(id).Props().Value("checksum", string.Empty);
                    stopWatch.Stop();
                    elapsed += stopWatch.ElapsedMilliseconds;
                });

                Debug.WriteLine("Read 1: " + elapsed);
                elapsed = 0;

                Parallel.For(0, 256, i =>
                {
                    var id = $"mech-{i}";
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    hive.Comb(id)
                        .Props()
                        .Refined("checksum", id);
                    stopWatch.Stop();
                    elapsed += stopWatch.ElapsedMilliseconds;
                });

                Debug.WriteLine("Update props: " + elapsed);
                elapsed = 0;

                Parallel.For(0, 256, (i) =>
                {
                    var id = $"mech-{i}";
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var prop = hive.Comb(id).Props().Value("checksum", string.Empty);
                    stopWatch.Stop();
                    elapsed += stopWatch.ElapsedMilliseconds;
                });

                Debug.WriteLine("Read 2: " + elapsed);
                elapsed = 0;

                Parallel.For(0, 256, i =>
                {
                    var id = $"mech-{i}";
                    var xoc = hive.Comb(id).Xocument("stuff.xml");
                    var name = xoc.Value($"/stuff/thing/text()", "");
                    xoc.Modify(new Directives().Xpath("//name").Set(Guid.NewGuid()));

                    var xoc2 = hive.Comb(id).Xocument("stuff.xml");
                    var name2 = xoc.Value($"/stuff/thing/text()", "");
                    xoc2.Modify(new Directives().Xpath("/stuff").AddIf("thing").Set(Guid.NewGuid()));

                    var xoc3 = hive.Comb(id).Xocument("stuff.xml");
                    var name3 = xoc.Value($"/stuff/thing/text()", "");
                    xoc3.Modify(new Directives().Xpath("/stuff").AddIf("thing").Set(Guid.NewGuid()));

                    Assert.Equal(1, hive.Shifted("machine").Comb(id).Xocument("stuff.xml").Nodes("//thing").Count);
                });


                Debug.WriteLine("Read Stuff: " + elapsed);
                elapsed = 0;
            }
        }

        [Fact]
        public void DeliversHQInParallelAfterShift()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName, "product");
                hive = hive.Shifted("still-parallel");

                var first = true;
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    if (!first)
                    {
                        hive.Catalog().Remove("X");
                        first = false;
                    }
                    hive.Catalog().Add("X");
                });
                Assert.True(hive.Catalog().Has("X"));
            }
        }

        [Fact]
        public void DeliversHQInParallel()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");

                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    var xoc = hive.HQ().Xocument("test");
                    xoc.Modify(
                        new Directives()
                            .Xpath("/test")
                            .AddIf("result")
                            .Set("passed")
                        );
                    Assert.Equal(
                        "passed",
                        hive.HQ().Xocument("test").Value("/test/result/text()", "")
                    );
                });
            }
        }

        [Fact]
        public void WritesPropsWithDecode()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName, "product");
                hive = hive.Shifted("still-parallel");

                hive.Comb("an id").Props().Refined("the,:prop", "the:,value");
                Assert.NotEqual(
                    "the,:prop:the:,value\r",
                    new TextOf(
                        hive.Comb("an id")
                        .Cell("props.cat")
                        .Content()
                    ).AsString()
                );
            }
        }

        [Fact]
        public void RemovesUnfilledComb()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");
                hive.Catalog().Add("2CV");
                hive.Catalog().Remove("2CV");
                Assert.Empty(
                    hive.Catalog().List()
                );
            }
        }
    }
}
