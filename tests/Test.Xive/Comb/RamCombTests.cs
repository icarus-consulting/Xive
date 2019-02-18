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

using System.Collections.Generic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Comb.Test
{
    public sealed class RamCombTests
    {
        [Fact]
        public void RemembersCell()
        {
            var memory = new Dictionary<string, byte[]>();
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
            var memory = new Dictionary<string, byte[]>();
            new RamComb("my-comb", memory)
                .Xocument("xoctor.xml")
                .Modify(
                    new Directives()
                        .Xpath("/xoctor")
                        .Set("help me please")
                );

            Assert.Equal(
                "help me please",
                new XMLCursor(
                    new InputOf(
                        new RamComb("my-comb", memory)
                            .Cell("xoctor.xml")
                            .Content()
                    )
                ).Values("/xoctor/text()")[0]
            );
        }

        [Fact]
        public void ListsContentSize()
        {
            using (var dir = new TempDirectory())
            {
                var combName = "combName";
                var cellName = "something.tmp";
                var comb = new RamComb(combName);
                

                using (var cell = comb.Cell(cellName))
                {
                    cell.Update(new InputOf("abc"));
                }

                using (var guts = comb.Cell("_guts.xml"))
                {
                    Assert.Equal(
                       "3",
                        new FirstOf<string>(
                            new XMLCursor(
                                new InputOf(
                                   guts.Content()
                                )
                            ).Values($"/items/item[name/text()='{combName}\\{cellName}']/size/text()")
                        ).Value()
                    );
                }
            }
        }
    }
}
