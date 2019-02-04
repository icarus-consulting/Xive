using System;
using System.IO;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public sealed class FileHiveTests
    {
        [Fact]
        public void DeliversHQ()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Equal(
                    $"product{Path.DirectorySeparatorChar}HQ",
                    new FileHive(
                        "product",
                        dir.Value().FullName
                    )
                    .HQ()
                    .Name()
                );
            }
        }

        [Fact]
        public void FindsComb()
        {
            using (var dir = new TempDirectory())
            {
                var dxm = new FileHive("product", dir.Value().FullName);
                new Catalog(dxm).Create("2CV");
                Assert.NotEmpty(
                    dxm.Combs("@id='2CV'")
                );
            }
        }

        [Fact]
        public void CreatesHiveFolder()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive("product", dir.Value().FullName);
                new Catalog(hive).Create("2CV");

                using (var cell = new FirstOf<IComb>(hive.Combs("@id='2CV'")).Value().Cell("Some testing item"))
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                    var productDir = Path.Combine(dir.Value().FullName, "product");
                    Assert.True(
                        Directory.Exists(productDir),
                        $"Directory '{productDir}' doesn't exist"
                    );
                }
            }
        }

        [Fact]
        public void CreatesCombFolder()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive("product", dir.Value().FullName);
                new Catalog(hive).Create("2CV");

                using (var cell =
                    new FirstOf<IComb>(
                        hive.Combs("@id='2CV'")
                    ).Value().Cell("Some testing item")
                )
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                    var combDir = Path.Combine(dir.Value().FullName, "product", "2CV");
                    Assert.True(
                        Directory.Exists(combDir),
                        $"Directory '{combDir}' doesn't exist"
                    );
                }
            }
        }

        [Fact]
        public void RemembersCombCell()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive("product", dir.Value().FullName);
                new Catalog(hive).Create("2CV");

                using (var cell =
                    new FirstOf<IComb>(
                        hive.Combs("@id='2CV'")
                    ).Value().Cell("Some testing item")
                )
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                }

                using (var cell =
                    new FirstOf<IComb>(
                        hive.Combs("@id='2CV'")
                    ).Value().Cell("Some testing item")
                )
                {
                    Assert.Equal(
                        "I am a very cool testdata string",
                        new TextOf(
                            cell.Content()
                        ).AsString()
                    );
                }
            }
        }

        [Fact]
        public void FailsOnEmptyDirectory()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Throws<ArgumentException>(() =>
                    new FileHive("product", "").Combs("@id='karre'")
                );
            }
        }

        [Fact]
        public void FailsOnMissingRootPath()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Throws<ArgumentException>(() =>
                    new FileHive("product", "dir").Combs("@id='karre'")
                );
            }
        }

        [Fact]
        public void HasCorrectName()
        {
            Assert.Equal("the name", new FileHive("the name", "the root").Name());
        }
    }
}
