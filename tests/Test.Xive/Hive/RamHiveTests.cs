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

using System.Collections.Generic;
using System.IO;
using Xive.Cache;
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
        public void DeliversComb()
        {
            var hive = new RamHive("in-memory");
            var catalog = new SimpleCatalog("in-memory", hive.HQ());
            catalog.Create("123");

            Assert.NotEmpty(catalog.List("@id='123'"));
        }

        [Fact]
        public void ShiftsScope()
        {
            var hive = new RamHive();
            var catalog = new SimpleCatalog(hive);
            catalog.Create("123");

            var shifted = hive.Shifted("twilight-zone");
            var twilightCatalog = new SimpleCatalog(shifted);

            Assert.Empty(twilightCatalog.List("@id='123'"));
        }

        [Fact]
        public void DistinguishesScope()
        {
            var mem = new SimpleMemories();
            var hive = new RamHive(mem);
            var catalog = new SimpleCatalog(hive);
            catalog.Create("123");

            var shifted = hive.Shifted("twilight-zone");
            var twilightCatalog = new SimpleCatalog(shifted);
            twilightCatalog.Create("789");

            Assert.Contains("twilight-zone/hq/catalog.xml", mem.XML().Knowledge());
        }

        [Fact]
        public void DeliversHQCell()
        {
            string expected = "Four headquarters are one head";
            var hive = new RamHive("in-memory");
            hive.HQ().Cell("catalog.xml").Update(new InputOf(expected));
            Assert.Equal(
                expected,
                new TextOf(
                    new InputOf(
                        hive.HQ().Cell("catalog.xml").Content()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void PrependsScopeToCombName()
        {
            IHive hive = new RamHive();

            var shifted = hive.Shifted("prepend-this");
            new SimpleCatalog(shifted).Create("an-entry");

            Assert.StartsWith("prepend-this",
                new FirstOf<IHoneyComb>(
                    shifted.Combs("@id='an-entry'")
                ).Value().Name()
            );
        }

        [Fact]
        public void DeliversHQXocument()
        {
            var hive = new RamHive("in-memory");
            hive.HQ().Xocument("catalog.xml")
                .Modify(
                    new Directives()
                        .Xpath("/catalog")
                        .Add("item")
                        .Attr("id", "8")
                        .Set("hello I am in a xocument, pretty cool eh?")
                );
            Assert.Equal(
                "8",
                hive.HQ().Xocument("catalog.xml").Value("/catalog/item[@id='8']/@id", "")
            );
        }

        [Fact]
        public void RemembersCombs()
        {
            var hive = new RamHive("animal");
            var catalog = new SimpleCatalog(hive);
            catalog.Create("123");
            catalog.Create("456");

            var comb = new FirstOf<IHoneyComb>(hive.Combs("@id='456'")).Value();
            comb.Cell("my-cell").Update(new InputOf("larva"));

            Assert.Equal(
                "larva",
                new TextOf(
                    new InputOf(
                        new FirstOf<IHoneyComb>(
                            hive.Combs("@id='456'")
                        ).Value()
                        .Cell("my-cell")
                        .Content()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void RemembersXocument()
        {
            var hive = new RamHive("animal");
            var catalog = new SimpleCatalog(hive);
            catalog.Create("123");
            catalog.Create("456");

            var comb = new FirstOf<IHoneyComb>(hive.Combs("@id='456'")).Value();
            comb.Xocument("meatloaf.xml")
                .Modify(
                    new Directives()
                    .Xpath("/meatloaf")
                    .Add("lines")
                    .Add("line").Set("And I would do anything for love").Up()
                    .Add("line").Set("But I won't do that").Up()
                );

            Assert.Contains(
                "But I won't do that",
                new FirstOf<IHoneyComb>(
                    hive.Combs("@id='456'")
                ).Value().Xocument("meatloaf.xml").Values("//line/text()")
            );
        }
    }
}
