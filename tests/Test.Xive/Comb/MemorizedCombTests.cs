using System;
using System.Collections.Generic;
using System.Text;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.Collection;
using Yaapii.Atoms.IO;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Comb.Test
{
    public sealed class MemorizedCombTests
    {
        [Fact]
        public void KnowledgeXmlOnce()
        {
            var comb = new MemorizedComb("the name", new RamMemories());
            comb.Cell("cell").Update(new InputOf("cool text"));
            comb.Xocument("xocument").Modify(new Directives().Xpath("xocument").Add("root").Set("also cool"));

            var values = comb.Xocument("_guts.xml").Values("/items/*/item/name/text()");
            Assert.Single(new Filtered<string>((text) => text.Equals("xocument"), values));
        }

        [Fact]
        public void KnowledgeDataOnce()
        {
            var comb = new MemorizedComb("the name", new RamMemories());
            comb.Cell("cell").Update(new InputOf("cool text"));
            comb.Xocument("xocument").Modify(new Directives().Xpath("xocument").Add("root").Set("also cool"));

            var values = comb.Xocument("_guts.xml").Values("/items/*/item/name/text()");
            Assert.Single(new Filtered<string>((text) => text.Equals("cell"), values));
        }

        [Fact]
        public void ReturnsNormalizedName()
        {
            var comb = new MemorizedComb("name\\with/diff", new RamMemories());
            Assert.Equal(
                "name/with/diff",
                comb.Name()
            );
        }

        [Fact]
        public void HasProps()
        {
            var name = "name";
            var comb = new MemorizedComb(name, new RamMemories());
            comb.Props().Refined("testprop", "first");
            Assert.Equal(
                "first",
                comb.Props().Value("testprop")
            );
        }

        [Fact]
        public void ReturnsGutsAsCell()
        {
            var comb = new MemorizedComb("comb", new RamMemories());
            comb.Cell("cell").Update(new InputOf("cool text"));
            Assert.Equal(
                "cell",
                new XMLSlice(new InputOf(comb.Cell("_guts.xml").Content())).Values("/items/data/item/name/text()")[0]
            );
        }

        [Fact]
        public void ReturnsItemsOnceRam()
        {
            var comb = new MemorizedComb("comb", new RamMemories());
            comb.Cell("cell").Update(new InputOf("cool text"));
            comb.Xocument("thexml.xml").Modify(new Directives().Xpath("/thexml").Add("newElement").Set("content"));
            Assert.Equal(
                "2",
                comb.Xocument("_guts.xml").Value("count(/items/*/item)", "0")
            );
        }

        [Fact]
        public void ReturnsItemsOnceFile()
        {
            using (var temp = new TempDirectory())
            {
                var comb = new MemorizedComb("comb", new FileMemories(temp.Value().FullName));
                comb.Cell("cell").Update(new InputOf("cool text"));
                comb.Xocument("thexml.xml").Modify(new Directives().Xpath("/thexml").Add("newElement").Set("content"));
                Assert.Equal(
                    "2",
                    comb.Xocument("_guts.xml").Value("count(/items/*/item)", "0")
                );
            }
        }
    }
}
