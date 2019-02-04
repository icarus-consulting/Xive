using System.Collections.Generic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Xive.Comb;

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
    }
}
