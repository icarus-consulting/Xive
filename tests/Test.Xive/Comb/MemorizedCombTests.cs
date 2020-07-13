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
        public void GutsIncludeXocuments()
        {
            var comb = new MemorizedComb("the name", new RamMemories2());
            comb.Xocument("xocument")
                .Modify(
                    new Directives()
                        .Xpath("xocument")
                        .Add("root")
                        .Set("also cool")
                );

            var values = 
                comb.Xocument("_guts.xml")
                    .Values("/items/item/name/text()");

            Assert.Single(new Filtered<string>((text) => text.Equals("xocument"), values));
        }

        [Fact]
        public void GutsIncludeData()
        {
            var comb = new MemorizedComb("the name", new RamMemories2());
            comb.Cell("cell").Update(new InputOf("cool text"));

            var values = 
                comb.Xocument("_guts.xml")
                    .Values("/items/item/name/text()");

            Assert.Single(new Filtered<string>((text) => text.Equals("cell"), values));
        }

        [Fact]
        public void NormalizesName()
        {
            var comb = new MemorizedComb("name\\with/diff", new RamMemories2());
            Assert.Equal(
                "name/with/diff",
                comb.Name()
            );
        }

        [Fact]
        public void HasProps()
        {
            var name = "name";
            var comb = new MemorizedComb(name, new RamMemories2());
            comb.Props().Refined("testprop", "first");
            Assert.Equal(
                "first",
                comb.Props().Value("testprop")
            );
        }

        [Fact]
        public void MergesIfNameEqual()
        {
            using (var temp = new TempDirectory())
            {
                var comb =
                    new MemorizedComb("comb",
                        new FileMemories2(temp.Value().FullName)
                    );

                comb.Cell("some-data")
                    .Update(new InputOf("<some-data/>"));

                comb.Xocument("some-data")
                    .Modify(
                        new Directives()
                            .Xpath("/some-data")
                            .Add("newElement").Set("content")
                        );

                Assert.Equal(
                    "1",
                    comb.Xocument("_guts.xml")
                        .Value("count(/items/item)", "0")
                );
            }
        }
    }
}
