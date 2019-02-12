using System;
using System.Threading.Tasks;
using Xive.Farm;
using Xive.Hive;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Test.Farm
{
    public class SyncFarmTest
    {
        [Fact]
        public void DeliversHive()
        {
            var farm = new SyncFarm(new RamFarm());
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                Assert.Equal(
                    "my-hive",
                    farm.Hive("my-hive").Name()
                );
            });
        }

        [Fact]
        public void RemembersCatalogChanges()
        {
            var farm = new SyncFarm(new RamFarm());
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                new Catalog(farm.Hive("person")).Create("123");
                Assert.Contains(
                    "123",
                    new Catalog(
                        farm.Hive("person")
                    ).List("@id='123'")
                );
            });
        }

        [Fact]
        public void RemembersCombChanges()
        {
            var farm = new SyncFarm(new RamFarm());
            new Catalog(farm.Hive("person")).Create("123");

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                var comb = new FirstOf<IHoneyComb>(
                    farm.Hive("person").Combs("@id='123'")
                ).Value();

                comb.Cell("address")
                    .Update(new InputOf("the moon"));

                Assert.Equal(
                    "the moon",
                    new TextOf(
                        new InputOf(
                            comb.Cell("address").Content()
                        )
                    ).AsString()
                );
            });
        }
    }
}
