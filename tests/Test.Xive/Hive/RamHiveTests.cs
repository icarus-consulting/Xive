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

using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public sealed class RamHiveTests
    {
        [Fact]
        public void RemembersComb()
        {
            var mem = new RamMemories();
            new MemorizedHive("in-memory", mem)
                .Catalog()
                .Add("123");

            Assert.Equal(
                "in-memory/123",
                 new MemorizedHive("in-memory", mem)
                    .Catalog()
                    .Filtered()[0]
                    .Name()
            );
        }

        [Fact]
        public void DeliversProps()
        {
            var mem = new RamMemories();
            var hive = new MemorizedHive("in-memory", mem);
            hive.Catalog()
                .Add("123");

            hive.Catalog().Filtered()[0]
            .Props()
            .Refined("prop", "eller");

            Assert.Equal(
                "eller",
                hive.Catalog().Filtered()[0]
                    .Props()
                    .Value("prop")
            );
        }

        [Fact]
        public void ShiftsScope()
        {
            var mem = new RamMemories();
            var hive = new MemorizedHive("in-memory", mem);
            hive.Catalog()
                .Add("123");

            var shifted = hive.Shifted("twilight-zone");
            Assert.Empty(shifted.Catalog().Filtered());
        }

        [Fact]
        public void DistinguishesScope()
        {
            var mem = new RamMemories();
            var hive = new MemorizedHive("in-memory", mem);
                hive.Catalog()
                    .Add("123");

            var shifted = hive.Shifted("twilight-zone");
            shifted.Catalog().Add("789");

            Assert.Contains("twilight-zone/hq/catalog.xml", mem.XML().Knowledge());
        }

        [Fact]
        public void DeliversHQCell()
        {
            string expected = "Four headquarters are one head";
            var mem = new RamMemories();
            var hive = new MemorizedHive("in-memory", mem);
                hive.Catalog()
                    .Add("123");

            hive.Comb("123", false).Props().Refined("Text of the day", expected);

            Assert.Contains(
                expected,
                new TextOf(
                    new InputOf(
                        hive.HQ().Xocument("catalog.xml").Node().ToString()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void PrependsScopeToCombName()
        {
            var mem = new RamMemories();
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
            var mem = new RamMemories();
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
            var mem = new RamMemories();
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
            var mem = new RamMemories();
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
    }
}
