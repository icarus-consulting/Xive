using System;
using System.Threading.Tasks;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Xive.Cell;

namespace Xive.Cell.Test
{
    public class SyncCellTest
    {
        [Fact]
        public void WorksInParallel()
        {
            var accesses = 0;
            var func = new Func<byte[]>(() =>
            {
                accesses++;
                Assert.Equal(1, accesses);
                accesses--;
                return new byte[0];
            });
            var cell = new SyncCell("TestCell", (path) => new FkCell(func, (content) => { func(); }));
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                cell.Content();
                cell.Update(new DeadInput());
            });
        }

        [Fact]
        public void WorksInParallelWithSameName()
        {
            var accesses = 0;
            var func = new Func<byte[]>(() =>
            {
                accesses++;
                Assert.Equal(1, accesses);
                accesses--;
                return new byte[0];
            });
            var cell1 = new SyncCell("testname", (path) => new FkCell(func, (content) => { }));
            var cell2 = new SyncCell("testname", (path) => new FkCell(func, (content) => { }));
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                cell1.Content();
                cell2.Content();
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
