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
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

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
                var comb = new FileComb(dir.Value().FullName, "my-comb");
                using (var cell = comb.Cell("Non existing"))
                {
                    Assert.InRange<int>(cell.Content().Length, 0, 0);
                }
            }
        }

        [Fact]
        public void DeliversXocument()
        {
            using (var dir = new TempDirectory())
            {
                var comb = new FileComb(dir.Value().FullName, "my-comb");
                using (var xoc = comb.Xocument("some.xml"))
                {
                    Assert.Equal(
                        1,
                        xoc.Nodes("/some").Count
                    );
                }
            }
        }

        [Fact]
        public void XocumentRootSkipsSubDir()
        {
            using (var dir = new TempDirectory())
            {
                var comb = new FileComb(dir.Value().FullName, "my-comb");
                using (var xoc = comb.Xocument("sub/some.xml"))
                {
                    Assert.Equal(
                        1,
                        xoc.Nodes("/some").Count
                    );
                }
            }
        }

        [Fact]
        public void XocumentCreatesSubDirectories()
        {
            using (var dir = new TempDirectory())
            {
                var comb =
                    new FileComb(
                        dir.Value().FullName,
                        "my-comb"
                    );

                using (var xoc = comb.Xocument("sub/xoc/ume/nt.xml"))
                {
                    xoc.Modify(
                        new Directives()
                            .Xpath("/nt")
                            .Add("NotWindowsNT")
                        );

                    Assert.True(
                        File.Exists(
                            Path.Combine(
                                dir.Value().FullName,
                                "my-comb",
                                "sub/xoc/ume/nt.xml"
                            )
                        )
                    );
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
                        dir.Value().FullName,
                        "my-comb"
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
                        dir.Value().FullName,
                        "my-comb"
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
                    using (var cell = new FileComb("d:/", "my-comb").Cell(""))
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
                    using (var cell = new FileComb("", "dir").Cell("file.txt"))
                    {
                        cell.Content();
                    }
                });
            }
        }
    }
}
