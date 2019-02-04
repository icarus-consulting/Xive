using Xunit;
using Xive;
using Xive.Cell;
using Xive.Comb;

namespace Xive.Test
{
    public sealed class SimpleCombTests
    {
        [Fact]
        public void DeliversCell()
        {
            var cell =
                new SimpleComb("my-cell", cellName => 
                    new RamCell(cellName)
                ).Cell("A non existing cell");

            Assert.InRange<int>(cell.Content().Length, 0, 0);
        }
    }
}
