using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Xive.Cell;

namespace Xive.Cell.Test
{
    public sealed class RamCellTests
    {
        [Fact]
        public void InitializesWithMemory()
        {
            using (var cell = new RamCell("my-cell", new byte[1] { 0x00 }))
            {
                Assert.True(
                    cell.Content()[0]
                        .Equals(0x00)
                );
            }
        }

        [Fact]
        public void CanUpdate()
        {
            using (var cell = new RamCell("my-cell"))
            {
                cell.Update(new InputOf("its so hot outside"));
                Assert.Equal(
                    "its so hot outside",
                    new TextOf(cell.Content()).AsString()
                );
            }
        }
    }
}
