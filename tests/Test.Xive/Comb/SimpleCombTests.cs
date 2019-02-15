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
