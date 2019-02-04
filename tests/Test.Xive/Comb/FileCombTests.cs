using System;
using System.IO;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Xive.Comb;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Comb.Test
{
    public sealed class FileCombTests
    {
        [Fact]
        public void DeliversCell()
        {
            using (var dir = new TempDirectory())
            {
                var comb = new FileComb("my-comb", dir.Value().FullName);
                using (var cell = comb.Cell("Non existing"))
                {
                    Assert.InRange<int>(cell.Content().Length, 0, 0);
                }
            }
        }

        [Fact]
        public void RemembersCell()
        {
            using (var dir = new TempDirectory())
            {
                var comb =
                    new FileComb(
                        "my-comb",
                        dir.Value().FullName
                    );
                using (var cell = comb.Cell("Interieur"))
                {
                    cell.Update(new InputOf("Green Seats"));
                }
                using (var cell = comb.Cell("Interieur"))
                {
                    Assert.Equal(
                        "Green Seats",
                        new TextOf(cell.Content()).AsString()
                    );
                }
            }
        }

        [Fact]
        public void CreatesSubDirectories()
        {
            using (var dir = new TempDirectory())
            {
                var comb =
                    new FileComb(
                        "my-comb",
                        dir.Value().FullName
                    );
                var path = "SubDir/Interieur";
                using (var cell = comb.Cell(path))
                {
                    cell.Update(new InputOf("content results in file creation"));
                    Assert.True(
                        File.Exists(
                            Path.Combine(
                                dir.Value().FullName,
                                "my-comb",
                                path
                            )
                        )
                    );
                }
            }
        }

        [Fact]
        public void CombNameWorks()
        {
            using (var dir = new TempDirectory())
            {
                var comb =
                    new FileComb(
                        "my-comb",
                        dir.Value().FullName
                    );
                Assert.Equal("my-comb", comb.Name());
            }
        }

        [Fact]
        public void FailsOnMissingFilename()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    using (var cell = new FileComb("my-comb", "d:/").Cell(""))
                    {
                        cell.Content();
                    }
                });
            }
        }

        [Fact]
        public void FailsOnMissingRootPath()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    using (var cell = new FileComb("dir", "").Cell("file.txt"))
                    {
                        cell.Content();
                    }
                });
            }
        }
#pragma warning restore MaxPublicMethodCount // a public methods count maximum
    }
}
