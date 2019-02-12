using Xive.Farm;
using Xive.Hive;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Test.Farm
{
    public sealed class FileFarmTests
    {
        [Fact]
        public void DeliversHive()
        {
            using (var dir = new TempDirectory())
            {
                var farm = new FileFarm(dir.Value().FullName);
                Assert.Equal(
                    "my-hive",
                    farm.Hive("my-hive").Name()
                );
            }
        }

        [Fact]
        public void RemembersCatalogChanges()
        {
            using (var dir = new TempDirectory())
            {
                var farm = new FileFarm(dir.Value().FullName);
                new Catalog(farm.Hive("person")).Create("123");

                Assert.Contains(
                    "123",
                    new Catalog(
                        farm.Hive("person")
                    ).List("@id='123'")
                );
            }
        }

        [Fact]
        public void RemembersCombChanges()
        {
            using (var dir = new TempDirectory())
            {
                var farm = new FileFarm(dir.Value().FullName);
                new Catalog(farm.Hive("person")).Create("123");
                var person =
                    new FirstOf<IHoneyComb>(
                        farm.Hive("person").Combs("@id='123'")
                    ).Value();

                person.Cell("address").Update(new InputOf("the moon"));

                Assert.Equal(
                    "the moon",
                    new TextOf(
                        new InputOf(
                            new FirstOf<IHoneyComb>(
                                farm.Hive("person").Combs("@id='123'")
                            ).Value().Cell("address").Content()
                        )
                    ).AsString()
                );
            }
        }
    }
}
