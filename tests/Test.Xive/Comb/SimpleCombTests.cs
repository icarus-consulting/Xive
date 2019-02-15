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

namespace Xive.Comb.Test
{
    public sealed class SimpleCombTests
    {
        [Fact]
        public void DeliversCell()
        {
            var result =
                new SimpleComb(
                    "my-cell", 
                    cellName => new RamCell(cellName),
                    (cellName, cell) => new CellXocument(cell, cellName)
                ).Cell("A-non-existing-cell");

            Assert.InRange(result.Content().Length, 0, 0);
        }

        [Fact]
        public void DeliversXocument()
        {
            var result =
                new SimpleComb(
                    "my-cell",
                    cellName => new RamCell(cellName),
                    (cellName, cell) => new CellXocument(cell, cellName)
                ).Xocument("this-is-a-xocument");

            Assert.Equal(
                1,
                result.Nodes("/this-is-a-xocument").Count
            );
        }
    }
}
