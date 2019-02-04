using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Hive.Test
{
    public sealed class RamHiveTests
    {
        [Fact]
        public void DeliversComb()
        {
            var hive = new RamHive("in-memory");
            var catalog = new Catalog("in-memory", hive.HQ());
            catalog.Create("123");

            Assert.NotEmpty(catalog.List("@id='123'"));
        }

        [Fact]
        public void RemembersCombs()
        {
            var hive = new RamHive("animal");
            var catalog = new Catalog(hive);
            catalog.Create("123");
            catalog.Create("456");

            var comb = new FirstOf<IComb>(hive.Combs("@id='456'")).Value();
            comb.Cell("my-cell").Update(new InputOf("larva"));

            Assert.Equal(
                "larva",
                new TextOf(
                    new InputOf(
                        new FirstOf<IComb>(
                            hive.Combs("@id='456'")
                        ).Value()
                        .Cell("my-cell")
                        .Content()
                    )
                ).AsString()
            );
        }
    }
}
