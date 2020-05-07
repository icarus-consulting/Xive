//MIT License

//Copyright (c) 2020 ICARUS Consulting GmbH

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

using Xive.Mnemonic;
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
        public void RemembersCell()
        {
            using (var temp = new TempDirectory())
            {
                new FileComb(temp.Value().FullName, "my-comb")
                    .Cell("my-cell")
                    .Update(new InputOf("larva"));

                Assert.Equal(
                    "larva",
                    new TextOf(
                        new InputOf(
                            new FileComb(temp.Value().FullName, "my-comb")
                                .Cell("my-cell")
                                .Content()
                        )
                    ).AsString()
                );
            }
        }

        [Fact]
        public void RemembersXocument()
        {
            using (var temp = new TempDirectory())
            {
                new FileComb(temp.Value().FullName, "my-comb")
                    .Xocument("xoctor.xml")
                    .Modify(
                        new Directives()
                            .Xpath("/xoctor")
                            .Add("request")
                            .Set("help me please")
                    );

                Assert.Equal(
                    "help me please",
                    new FileComb(temp.Value().FullName, "my-comb")
                        .Xocument("xoctor.xml")
                        .Values("/xoctor/request/text()")[0]
                );
            }
        }

        [Fact]
        public void RemembersProps()
        {
            using (var temp = new TempDirectory())
            {
                new FileComb(temp.Value().FullName, "my-comb")
                    .Props()
                    .Refined("beer", "astra");

                Assert.Equal(
                    "astra",
                    new FileComb(temp.Value().FullName, "my-comb")
                        .Props()
                        .Value("beer")
                );
            }
        }

        [Fact]
        public void DeliversXocument()
        {
            using (var temp = new TempDirectory())
            {
                var comb = new FileComb(temp.Value().FullName, "my-comb");
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
        public void RemembersSubPathXocument()
        {
            using (var temp = new TempDirectory())
            {
                var comb = new FileComb(temp.Value().FullName, "my-comb");
                using (var xoc = comb.Xocument("sub/dir/some.xml"))
                {
                    xoc.Modify(new Directives().Xpath("/some").Add("test"));
                }
                using (var xoc = comb.Xocument("sub/dir/some.xml"))
                {
                    Assert.Equal(
                        1,
                        xoc.Nodes("/some/test").Count
                    );
                }
            }
        }

        [Fact]
        public void XocumentRootSkipsSubDir()
        {
            using (var temp = new TempDirectory())
            {
                var comb = new FileComb(temp.Value().FullName, "my-comb");
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
        public void ReturnsGutsCaseAndSeparatorInsensitive()
        {
            using (var temp = new TempDirectory())
            {
                var memory = new FileMemories(temp.Value().FullName);
                var comb = new FileComb("my-comb", memory);
                using (var xoc = comb.Xocument("sub\\DIR/some.xml"))
                {
                    xoc.Modify(new Directives().Xpath("/some").Add("test"));
                }
                using (var xoc = comb.Xocument("_guts.xml"))
                {
                    Assert.Equal(
                        @"sub/dir/some.xml",
                        xoc.Value("/items/xml/item/name/text()", "")
                    );
                }
            }
        }
    }
}
