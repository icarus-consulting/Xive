//MIT License

//Copyright (c) 2019 ICARUS Consulting GmbH

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.IO;
using System.Text;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Cell.Test
{
    public sealed class FileCellTests
    {
        [Theory]
        [InlineData("UTF-7")]
        [InlineData("UTF-8")]
        [InlineData("UTF-16")]
        [InlineData("UTF-32")]
        public void WorksWithEncoding(string name)
        {
            var encoding = Encoding.GetEncoding(name);
            var inBytes = encoding.GetBytes("Can or can't I dö prüpär äncöding?");

            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, "itemFile.tmp")))
            {
                item.Update(new InputOf(inBytes));

                Assert.Equal(
                    "Can or can't I dö prüpär äncöding?",
                    new TextOf(
                        new InputOf(inBytes),
                        encoding
                    ).AsString()
                );
            }
        }

        [Fact]
        public void ReadsWithoutContent()
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
        public void RejectsWhiteSpacesFilename()
        {
            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, $"white space folder/white space file name")))
            {
                Assert.Throws<ArgumentException>(
                    () => item.Update(new InputOf("after holiday is before holiday"))
                );
            }
        }

        [Fact]
        public void AllowsWhiteSpacesFoldername()
        {
            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, $"white space folder/filename.txt")))
            {
                item.Update(new InputOf("after holiday is before holiday"));
                Assert.True(
                    File.Exists(Path.Combine(dir.Value().FullName, "white space folder", "filename.txt"))
                );
            }
        }

        [Fact]
        public void AllowBackslash()
        {
            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, $"someFolder/filename.txt")))
            {
                var create =  Path.Combine(dir.Value().FullName, $"someFolder/filename.txt");
                var search = Path.Combine(dir.Value().FullName, "someFolder", "filename.txt");
                Console.Write(create);
                Console.Write(search);
                Console.Write("eqal:" + (search.Equals(create)).ToString());

                item.Update(new InputOf("after holiday is before holiday"));
                Assert.True(false);
                  //  File.Exists(Path.Combine(dir.Value().FullName, "someFolder", "filename.txt"))
                //);
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
                using (var cell = new FileCell("test.txt"))
                {
                    cell.Content();
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

        [Fact]
        public void ResetsStreamAfterUpdate()
        {
            using (var tmp = new TempDirectory())
            {
                using (var cell = new FileCell(Path.Combine(tmp.Value().FullName, "test.txt")))
                {
                    var content = new InputOf("its so hot outside");
                    cell.Update(content);
                    Assert.Equal(0, content.Stream().Position);
                }
            }
        }
    }
}
