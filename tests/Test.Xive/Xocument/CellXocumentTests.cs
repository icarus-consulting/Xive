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

using Xive.Cell;
using Xive.Xocument;
using Xunit;
using Yaapii.Xambly;

namespace Xive.Test.Xocument
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
    }
}
