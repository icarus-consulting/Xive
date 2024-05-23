//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public sealed class MemorizedHiveTests
    {
        [Fact]
        public void RemembersComb()
        {
            var mem = new RamMnemonic();
            new MemorizedHive("in-memory", mem)
                .Catalog()
                .Add("123");

            Assert.Equal(
                "in-memory/123",
                 new MemorizedHive("in-memory", mem)
                    .Catalog()
                    .List()[0]
                    .Name()
            );
        }

        [Fact]
        public void DeliversProps()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("in-memory", mem);
            hive.Catalog()
                .Add("123");

            hive.Catalog().List()[0]
            .Props()
            .Refined("prop", "eller");

            Assert.Equal(
                "eller",
                hive.Catalog().List()[0]
                    .Props()
                    .Value("prop")
            );
        }

        [Fact]
        public void ShiftsScope()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("in-memory", mem);
            hive.Catalog()
                .Add("123");

            var shifted = hive.Shifted("twilight-zone");
            Assert.Empty(shifted.Catalog().List());
        }

        [Fact]
        public void RejectsEmptyScope()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("", mem);
            AssertException.MessageMatches<ArgumentException>(
                "Unable to shift memory, because empty scopes are not allowed.",
                () => hive.Catalog()
            );
        }

        [Fact]
        public void DistinguishesScope()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("in-memory", mem);
            hive.Catalog()
                .Add("123");

            var shifted = hive.Shifted("twilight-zone");
            shifted.Catalog().Add("789");

            Assert.Contains("twilight-zone/hq/catalog.cat", mem.Contents().Knowledge(""));
        }

        [Fact]
        public void PrependsScopeToCombName()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("prepend-this", mem);
            hive.Catalog()
                .Add("123");

            var shifted = hive.Shifted("prepend-this");

            Assert.StartsWith("prepend-this",
                hive.Comb("123", false).Name()
            );
        }

        [Fact]
        public void DeliversHQXocument()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("in-memory", mem);
            hive.HQ().Cell("A").Update(new InputOf(new byte[1] { 0xAB }));

            Assert.Equal(
                new byte[1] { 0xAB },
                hive.HQ().Cell("A").Content()
            );
        }

        [Fact]
        public void RemembersCombs()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("in-memory", mem);
            hive.Catalog().Add("123");
            hive.Catalog().Add("456");

            hive.Comb("123", false)
                .Cell("my-cell")
                .Update(new InputOf("larva"));

            Assert.Equal(
                "larva",
                new TextOf(
                    new InputOf(
                        hive.Comb("123", false)
                        .Cell("my-cell")
                        .Content()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void RemembersXocument()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("animal", mem);
            hive.Catalog().Add("123");
            hive.Catalog().Add("456");

            hive.Comb("456").Xocument("meatloaf.xml")
                .Modify(
                    new Directives()
                        .Xpath("/meatloaf")
                        .Add("lines")
                        .Add("line").Set("And I would do anything for love").Up()
                        .Add("line").Set("But I won't do that").Up()
                );

            Assert.Contains(
                "But I won't do that",
                hive.Comb("456")
                    .Xocument("meatloaf.xml")
                    .Values("//line/text()")
            );
        }

        [Fact]
        public void AddsInParallel()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("cars", mem);

            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                hive.Catalog().Add("supercar");
            });

            Assert.Equal(1, hive.Catalog().List().Count);
        }

        [Fact]
        public void WritesPropsInParallel()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("cars", mem);

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.Comb("2CV").Props().Refined("looping", "louie");
            });

            Assert.Equal("louie", hive.Comb("2CV").Props().Value("looping"));
        }

        [Fact]
        public void WritesPropsWhenShifted()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("X", mem);

            Parallel.For(0, 256, (i) =>
            {
                var id = $"mech-{i}";
                hive.Shifted("machine").Catalog().Add(id);
                hive.Shifted("machine").Comb(id).Props().Refined("checksum", id);
            });

            Parallel.ForEach(hive.Shifted("machine").Catalog().List(), comb =>
            {
                Assert.Equal(comb.Name(), $"machine/{comb.Props().Value("checksum")}");
            });
        }

        [Fact]
        public void WritesComplexInParallel()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("X", mem);

            Parallel.For(0, 256, i =>
            {
                var id = $"mech-{i}";
                hive.Shifted("machine").Catalog().Add(id);
                hive.Shifted("machine").Comb(id).Props().Refined("checksum", id);
            });

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
            var mem = new RamMnemonic();
            IHive hive = new MemorizedHive("cars", mem);
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

        [Fact]
        public void DeliversHQInParallel()
        {
            var mem = new RamMnemonic();
            var hive = new MemorizedHive("cars", mem);

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
