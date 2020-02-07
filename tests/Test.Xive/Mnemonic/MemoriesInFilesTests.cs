﻿using Xive.Comb;
using Xive.Hive;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

namespace Xive.Test.Mnemonic
{
    public sealed class MemoriesInFilesTests
    {
        [Fact]
        public void MemorizesProps()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new MemoriesInFiles(temp.Value().FullName);
                var comb = new RamComb("beverage/HQ", mem);
                new SimpleCatalog("beverage", comb).Create("fritz-kola");

                mem.Props("beverage", "fritz-kola").Refined("light", "yes please");

                mem = new MemoriesInFiles(temp.Value().FullName);
                comb = new RamComb("beverage/HQ", mem);
                Assert.Equal(
                    "yes please",
                    mem.Props("beverage", "fritz-kola").Value("light")
                );
            }
        }

        [Fact]
        public void MemorizesXml()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new MemoriesInFiles(temp.Value().FullName);
                using (var xoc = new RamComb("beverage", mem).Xocument("coke.xml"))
                {
                    xoc.Modify(new Directives().Xpath("/coke").Add("light").Set("yes please"));
                }

                mem = new MemoriesInFiles(temp.Value().FullName);
                using (var xoc = new RamComb("beverage", mem).Xocument("coke.xml"))
                {
                    Assert.Equal(
                        "yes please",
                        xoc.Value("/coke/light/text()", "")
                    );
                }
            }
        }

        [Fact]
        public void MemorizesBinary()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new MemoriesInFiles(temp.Value().FullName);
                using (var pepsi = new RamComb("beverage", mem).Cell("pepsi"))
                {
                    pepsi.Update(new InputOf("Empty"));
                }

                mem = new MemoriesInFiles(temp.Value().FullName);
                using (var pepsi = new RamComb("beverage", mem).Cell("pepsi"))
                {
                    Assert.Equal(
                        "Empty",
                        new TextOf(
                            new InputOf(pepsi.Content())
                        ).AsString()
                    );
                }
            }
        }
    }
}
