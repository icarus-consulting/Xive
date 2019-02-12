using System.Collections.Generic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

namespace Xive.Hive.Test
{
    public sealed class RamHiveTests
    {
        [Fact]
        public void DeliversComb()
        {
            var mem = new Dictionary<string, byte[]>();
            var hive = new RamHive("in-memory", mem);
            var catalog = new Catalog("in-memory", hive.HQ());
            catalog.Create("123");

            Assert.NotEmpty(catalog.List("@id='123'"));
        }

        [Fact]
        public void DeliversHQCell()
        {
            var mem = new Dictionary<string, byte[]>();
            string expected = "Four headquarters are one head";
            var hive = new RamHive("in-memory", mem);
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
        public void DeliversHQXocument()
        {
            var mem = new Dictionary<string, byte[]>();
            var hive = new RamHive("in-memory", mem);
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
            var catalog = new Catalog(hive);
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
            var catalog = new Catalog(hive);
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
