using System.Collections.Generic;
using System.IO;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Xive.Comb;

namespace Xive.Hive.Test
{
    public sealed class SimpleHiveTests
    {
        [Fact]
        public void DeliversHQ()
        {
            var mem = new Dictionary<string, byte[]>();
            var hq =
                new SimpleHive(
                    "person", 
                    comb => new RamComb(comb, mem)
                ).HQ();

            Assert.Equal($"person{Path.DirectorySeparatorChar}HQ", hq.Name());
        }

        [Fact]
        public void RemembersHQ()
        {
            var mem = new Dictionary<string, byte[]>();
            var hq =
                new SimpleHive(
                    "person",
                    comb => new RamComb(comb, mem)
                ).HQ();

            hq.Cell("my-cell").Update(new InputOf("larva"));

            Assert.Equal(
                "larva",
                new TextOf(
                    new InputOf(
                        hq.Cell("my-cell").Content()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void DeliversComb()
        {
            var mem = new Dictionary<string, byte[]>();
            var hq =
                new SimpleHive(
                    "person",
                    comb => new RamComb(comb, mem)
                ).HQ();

            var catalog = new Catalog("person", hq);
            catalog.Create("Alfred");

            Assert.Contains("Alfred", catalog.List("@id='Alfred'"));
        }
    }
}
