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

using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Comb.Test
{
    public sealed class RamCombTests
    {
        [Fact]
        public void RemembersCell()
        {
            var memory = new RamMemories();
            new RamComb("my-comb", memory)
                .Cell("my-cell")
                .Update(new InputOf("larva"));

            Assert.Equal(
                "larva",
                new TextOf(
                    new InputOf(
                        new RamComb("my-comb", memory)
                            .Cell("my-cell")
                            .Content()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void RemembersXocument()
        {
            var memory = new RamMemories();
            new RamComb("my-comb", memory)
                .Xocument("xoctor.xml")
                .Modify(
                    new Directives()
                        .Xpath("/xoctor")
                        .Set("help me please")
                );

            Assert.Equal(
                "help me please",
                new RamComb("my-comb", memory)
                    .Xocument("xoctor.xml")
                    .Values("/xoctor/text()")[0]
            );
        }

        [Fact]
        public void RemembersProps()
        {
            var memory = new RamMemories();
            new RamComb("my-comb", memory)
                .Props()
                .Refined("beer", "astra");

            Assert.Equal(
                "astra",
                new RamComb("my-comb", memory)
                    .Props()
                    .Value("beer")
            );
        }

        [Fact]
        public void DeliversXocument()
        {
            var memory = new RamMemories();
            var comb = new RamComb("my-comb", memory);
            using (var xoc = comb.Xocument("some.xml"))
            {
                Assert.Equal(
                    1,
                    xoc.Nodes("/some").Count
                );
            }
        }

        [Fact]
        public void RemembersSubPathXocument()
        {
            var memory = new RamMemories();
            var comb = new RamComb("my-comb", memory);
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

        [Fact]
        public void XocumentRootSkipsSubDir()
        {
            var memory = new RamMemories();
            var comb = new RamComb("my-comb", memory);
            using (var xoc = comb.Xocument("sub/some.xml"))
            {
                Assert.Equal(
                    1,
                    xoc.Nodes("/some").Count
                );
            }
        }

        [Fact(Skip ="Guts are not supported at the moment")]
        public void ReturnsGutsCaseAndSeparatorInsensitive()
        {
            var memory = new RamMemories();
            var comb = new RamComb("my-comb", memory);
            using (var xoc = comb.Xocument("sub\\DIR/some.xml"))
            {
                xoc.Modify(new Directives().Xpath("/some").Add("test"));
            }
            using (var xoc = comb.Xocument("_guts.xml"))
            {
                Assert.Equal(
                    "sub/dir/some.xml",
                    xoc.Value("/items/item/name/text()", "")
                );
            }
        }
    }
}
