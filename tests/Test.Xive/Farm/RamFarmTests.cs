using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Xive;
using Xive.Farm;
using Xive.Hive;

namespace Xive.Test.Farm
{
    public sealed class RamFarmTests
    {
        [Fact]
        public void DeliversHive()
        {
            var farm = new RamFarm();

            Assert.Equal(
                "my-hive",
                farm.Hive("my-hive").Name()
            );
        }

        [Fact]
        public void RemembersCatalogChanges()
        {
            var farm = new RamFarm();
            new Catalog(farm.Hive("person")).Create("123");

            Assert.Contains(
                "123",
                new Catalog(
                    farm.Hive("person")
                ).List("@id='123'")
            );
        }

        [Fact]
        public void RemembersCombChanges()
        {
            var farm = new RamFarm();
            new Catalog(farm.Hive("person")).Create("123");
            var person =
                new FirstOf<IComb>(
                    farm.Hive("person").Combs("@id='123'")
                ).Value();

            person.Cell("address").Update(new InputOf("the moon"));

            Assert.Equal(
                "the moon",
                new TextOf(
                    new InputOf(
                        new FirstOf<IComb>(
                            farm.Hive("person").Combs("@id='123'")
                        ).Value().Cell("address").Content()
                    )
                ).AsString()
            );
        }
    }
}
