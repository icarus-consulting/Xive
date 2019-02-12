using System.Collections.Generic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Comb.Test
{
    public sealed class RamCombTests
    {
        [Fact]
        public void RemembersCell()
        {
            var memory = new Dictionary<string, byte[]>();
            new RamComb("my-comb", memory)
                .Cell("my-cell")
                .Update(new InputOf("larva"));

            Assert.Equal(
                "larva",
                new TextOf(
                    new InputOf(
                        new RamComb("my-comb", memory)
                            .Cell("my-cell")
                            .Content()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void RemembersXocument()
        {
            var memory = new Dictionary<string, byte[]>();
            new RamComb("my-comb", memory)
                .Xocument("xoctor.xml")
                .Modify(
                    new Directives()
                        .Xpath("/xoctor")
                        .Set("help me please")
                );

            Assert.Equal(
                "help me please",
                new XMLQuery(
                    new InputOf(
                        new RamComb("my-comb", memory)
                            .Cell("xoctor.xml")
                            .Content()
                    )
                ).Values("/xoctor/text()")[0]
            );
        }
    }
}
