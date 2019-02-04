using System;
using System.IO;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Xive.Cell;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Cell.Test
{
    public sealed class FileCellTests
    {
        [Fact]
        public void Works()
        {
            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, "itemFile.tmp")))
            {
                Assert.Empty(item.Content());
            }
        }

        [Fact]
        public void CanUpdate()
        {
            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, "niceFileName")))
            {
                item.Update(new InputOf("after holiday is before holiday"));
                Assert.Equal(
                    "after holiday is before holiday",
                    new TextOf(item.Content()).AsString()
                );
            }
        }

        [Fact]
        public void RemovesEmptyFile()
        {
            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, "niceFileName")))
            {
                item.Update(new InputOf("after holiday is before holiday"));
                item.Update(new InputOf(String.Empty));
                Assert.False(
                    File.Exists(dir.Value().FullName)
                );
            }
        }

        [Fact]
        public void HandlesWhiteSpaces()
        {
            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, $"white space folder{Path.AltDirectorySeparatorChar}white space file name")))
            {
                item.Update(new InputOf("after holiday is before holiday"));
                Assert.True(
                    File.Exists(Path.Combine(dir.Value().FullName, "white space folder", "white space file name"))
                );
            }
        }

        [Fact]
        public void FailsOnMissingFilename()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                using (var file = new FileCell("d:/"))
                {
                    file.Content();
                }
            }
            );
        }

        [Fact]
        public void FailsOnMissingRootPath()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                using (var file = new FileCell("test.txt"))
                {
                    file.Content();
                }
            });
        }


        [Fact]
        public void FailsOnEmptyPath()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                using (var file = new FileCell(""))
                {
                    file.Content();
                }
            });
        }
    }
}
