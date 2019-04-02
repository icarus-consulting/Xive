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

using System.IO;
using System.Text;
using Xive.Cell;
using Xive.Xocument;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

namespace Xive.Xocument.Test
{
    public sealed class CellXocumentTests
    {
        [Fact]
        public void DeliversContent()
        {
            Assert.Equal(
                "<flash />",
                new CellXocument(
                    new RamCell("flash.xml"),
                    "flash.xml"
                ).Node().ToString()
            );
        }

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
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, "encoded.tmp")))
            {
                var xoc =
                    new CellXocument(
                        item,
                        "encoded.xml"
                    );

                xoc.Modify(
                    new Directives()
                        .Xpath("encoded")
                        .Set(
                            new TextOf(
                                new InputOf(inBytes),
                                encoding
                            ).AsString()
                        )
                    );

                xoc =
                    new CellXocument(
                        item,
                        "encoded.xml"
                    );

                Assert.Equal(
                    "Can or can't I dö prüpär äncöding?",
                    xoc.Value("/encoded/text()", string.Empty)
                );
            }
        }

        [Fact]
        public void FileHasHeader()
        {
            using (var dir = new TempDirectory())
            using (var item = new FileCell(Path.Combine(dir.Value().FullName, "flash.xml")))
            {
                var file = Path.Combine(dir.Value().FullName, "flash.xml");
                new CellXocument(
                    item,
                    "flash.xml"
                ).Node();

                Assert.Equal(
                    "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>\r\n<flash />",
                    new TextOf(
                        new InputOf(new FileInfo(file))
                    ).AsString()
                );
            }
        }

        [Fact]
        public void UpdatesContent()
        {
            var xoc =
                new CellXocument(
                    new RamCell("flash.xml"),
                    "flash.xml"
                );

            xoc.Modify(
                new Directives()
                    .Xpath("/flash")
                    .Add("grandmaster").Set("scratch scratch")
            );

            Assert.Equal("scratch scratch", xoc.Value("/flash/grandmaster/text()", ""));
        }

        [Theory]
        [InlineData("UTF-7")]
        [InlineData("UTF-8")]
        [InlineData("UTF-16")]
        [InlineData("UTF-32")]
        public void DealsWithEncodings(string name)
        {
            var encoding = Encoding.GetEncoding(name);
            using (var xoc =
               new CellXocument(
                   new RamCell("flash.xml"),
                   "flash.xml",
                   encoding
               )
            )
            {

                xoc.Modify(
                    new Directives()
                        .Xpath("/flash")
                        .Add("grandmaster")
                        .Set("üöä")
                );

                Assert.Equal(
                    "üöä",
                    xoc.Value("/flash/grandmaster/text()", "")
                );
            }
        }

    }
}
