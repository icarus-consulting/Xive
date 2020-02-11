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
using System.Threading.Tasks;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public sealed class RamHiveTests
    {
        [Fact]
        public void DeliversHQ()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Equal(
                    $"product/hq",
                    new RamHive("product")
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
                var hive = new RamHive("product");
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
                var hive = new RamHive("cockpit");
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
                var hive = new RamHive("product");
                var shifted = hive.Shifted("prepend-this");
                shifted.Catalog().Add("an-entry");

                Assert.StartsWith("prepend-this",
                    shifted.Comb("an-entry").Name()
                );
            }
        }

        [Fact]
        public void ShiftsScope()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new RamHive("product");
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
        public void RemembersCombCell()
        {
            IHive hive = new RamHive("product");
            using (var cell = hive.Comb("2CV").Cell("Some-testing-item"))
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


        [Fact]
        public void DeliversScope()
        {
            Assert.Equal("the name", new RamHive("the name").Scope());
        }

        [Fact]
        public void AddsInParallel()
        {
            var valve = new SyncGate();
            var hive = new RamHive("testRamHive");

            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                hive.Catalog().Add("123");
            });

            Assert.Equal(1, hive.Catalog().List().Count);
        }

        [Fact]
        public void WritesPropsInParallel()
        {
            var hive = new RamHive("product");
            hive.Catalog().Add("2CV");

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.Comb("2CV").Props().Refined("looping", "louie");
            });

            Assert.Equal("louie", hive.Comb("2CV").Props().Value("looping"));
        }

        [Fact]
        public void WritesPropsWhenShifted()
        {
            var hive = new RamHive("product");
            for (int i = 0; i < 256; i++)
            {
                var id = $"mech-{i}";
                hive.Shifted("machine").Catalog().Add(id);
                hive.Shifted("machine").Comb(id).Props().Refined("checksum", id);
            }

            foreach (var comb in hive.Shifted("machine").Catalog().List())
            {
                Assert.Equal(comb.Name(), $"machine/{comb.Props().Value("checksum")}");
            }

        }

        [Fact]
        public void WritesComplexInParallel()
        {
            var hive = new RamHive("product");
            for (int i = 0; i < 256; i++)
            {
                var id = $"mech-{i}";
                hive.Shifted("machine").Catalog().Add(id);
                hive.Shifted("machine").Comb(id).Props().Refined("checksum", id);
            }

            Parallel.For(0, 256, i =>
            {
                var id = $"mech-{i}";
                using (var xoc = hive.Shifted("machine").Comb(id).Xocument("stuff.xml"))
                {
                    var name = xoc.Value($"/stuff/thing/text()", "");
                    xoc.Modify(new Directives().Xpath("//name").Set(Guid.NewGuid()));
                    using (var xoc2 = hive.Shifted("machine").Comb(id).Xocument("stuff.xml"))
                    {
                        var name2 = xoc.Value($"/stuff/thing/text()", "");
                        xoc2.Modify(new Directives().Xpath("/stuff").AddIf("thing").Set(Guid.NewGuid()));
                        using (var xoc3 = hive.Shifted("machine").Comb(id).Xocument("stuff.xml"))
                        {
                            var name3 = xoc.Value($"/stuff/thing/text()", "");
                            xoc3.Modify(new Directives().Xpath("/stuff").AddIf("thing").Set(Guid.NewGuid()));
                        }
                    }
                }
                Assert.Equal(1, hive.Shifted("machine").Comb(id).Xocument("stuff.xml").Nodes("//thing").Count);
            });

        }

        [Fact]
        public void DeliversHQInParallelAfterShift()
        {
            var hive = new RamHive("person").Shifted("still-parallel");
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

        [Fact]
        public void DeliversHQInParallel()
        {
            var hive = new RamHive("person");
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
}