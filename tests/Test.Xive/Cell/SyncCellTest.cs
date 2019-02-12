using System;
using System.Threading.Tasks;
using Xive.Test;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Cell.Test
{
    public class SyncCellTest
    {
        [Fact]
        public void WorksInParallel()
        {
            var accesses = 0;
            var cell = 
                new SyncCell(
                    "TestCell", 
                    (path) => new FkCell(
                        (content) => { },
                        () => {
                            accesses++;
                            Assert.Equal(1, accesses);
                            accesses--;
                            return new byte[0];
                        }
                    )
                );
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                cell.Content();
            });
        }

        [Fact]
        public void WorksInParallelWithSameName()
        {
            var accesses = 0;
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                new SyncCell(
                    "TestCell",
                    (path) => new FkCell(
                        (content) => { },
                        () => {
                            accesses++;
                            Assert.Equal(1, accesses);
                            accesses--;
                            return new byte[0];
                        }
                    )
                );
                new SyncCell(
                    "TestCell",
                    (path) => new FkCell(
                        (content) => { },
                        () => {
                            accesses++;
                            Assert.Equal(1, accesses);
                            accesses--;
                            return new byte[0];
                        }
                    )
                );
            });
        }

        [Fact]
        public void WorksWithRamCell()
        {
            using (var cell = 
                new SyncCell("my-cell", (name) => 
                    new RamCell(name)))
            {
                cell.Update(new InputOf("its so hot outside"));
                Assert.Equal(
                    "its so hot outside",
                    new TextOf(cell.Content()).AsString()
                );
            }
        }

        [Fact]
        public void WorksWithFileCell()
        {
            using(var file = new TempFile())
            using (var cell = 
                new SyncCell(file.Value(), (path) => 
                    new FileCell(path)))
            {
                cell.Update(new InputOf("Ram cell Input"));
                Assert.Equal("Ram cell Input", new TextOf(cell.Content()).AsString());
            }
        }
    }
}
