using System;
using System.Threading.Tasks;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Comb.Test
{
    public class SyncCombTests
    {
        [Fact]
        public void WorksInParallel()
        {
            var comb = 
                new SyncComb(
                    new RamComb("my-comb")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                comb.Cell("syncCell").Content();
                comb.Cell("syncCell").Update(new DeadInput());
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
            var comb1 = new SyncComb(new RamComb("my-comb"));
            var comb2 = new SyncComb(new RamComb("my-comb"));
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                comb1.Cell("syncCell").Content();
                comb2.Cell("syncCell").Content();
                comb1.Cell("syncCell").Update(new DeadInput());
                comb2.Cell("syncCell").Update(new DeadInput());
            });
        }

        [Fact]
        public void WorksWithFileCell()
        {
            using (var file = new TempDirectory())
            {
                var comb = 
                    new SyncComb(
                        new FileComb(
                            "myFileComb", 
                            file.Value().FullName
                        )
                    );

                using (var cell = comb.Cell("myCell"))
                {
                    Parallel.For(0, Environment.ProcessorCount << 4, i =>
                    {
                        cell.Update(new InputOf("cell content"));
                        Assert.Equal("cell content", new TextOf(cell.Content()).AsString());
                    });
                }
            }
        }

        [Fact]
        public void WorksWithRamCell()
        {
            var comb = 
                new SyncComb(
                    new RamComb("myRamComb")
                );

            using (var cell = comb.Cell("myCell"))
            {
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    cell.Update(new InputOf("cell content"));
                    Assert.Equal("cell content", new TextOf(cell.Content()).AsString());
                });
            }
        }
    }
}
