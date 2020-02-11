//MIT License

//Copyright (c) 2020 ICARUS Consulting GmbH

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
        public void AccessIsExclusive()
        {
            var accesses = 0;
            var cell =
                new FkCell(
                    (content) => { },
                    () =>
                    {
                        accesses++;
                        Assert.Equal(1, accesses);
                        accesses--;
                        return new byte[0];
                    }
                );

            var valve = new SyncGate();
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                using (var mutexed = new SyncCell(cell, valve))
                {
                    mutexed.Content();
                }
            });
        }

        [Fact]
        public void WorksParallel()
        {
            var cell = new RamCell();
            var gate = new SyncGate();
            Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
            {
                var content = Guid.NewGuid().ToString();
                using (var mutexed = new SyncCell(cell, gate))
                {
                    mutexed.Update(new InputOf(content));
                    Assert.Equal(content, new TextOf(mutexed.Content()).AsString());
                }
            });
        }

        [Fact]
        public void DoesNotBlockItself()
        {
            var cell = new RamCell();
            var gate = new SyncGate();
            Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
            {
                var content = Guid.NewGuid().ToString();
                using (var mutexed = new SyncCell(cell, gate))
                {
                    mutexed.Update(new InputOf(content));
                    using (var again = new SyncCell(cell, gate))
                    {
                        again.Update(new InputOf(content));
                        using (var andAgain = new SyncCell(cell, gate))
                        {
                            andAgain.Update(new InputOf(content));
                            System.Threading.Thread.Sleep(1);
                        }
                    }
                    Assert.Equal(content, new TextOf(mutexed.Content()).AsString());
                }
            });
        }

        [Fact]
        public void WorksWithRamCell()
        {
            var cell = new RamCell();
            var gate = new SyncGate();
            using (var mutexed = new SyncCell(cell, gate))
            {
                mutexed.Update(new InputOf("its so hot outside"));
                Assert.Equal(
                    "its so hot outside",
                    new TextOf(mutexed.Content()).AsString()
                );
            }
        }

        [Fact]
        public void WorksWithFileCell()
        {
            using (var file = new TempFile())
            {
                var path = file.Value();
                var gate = new SyncGate();
                Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
                {
                    using (var cell = new SyncCell(new FileCell(path), gate))
                    {
                        cell.Update(new InputOf("Ram cell Input"));
                        Assert.Equal("Ram cell Input", new TextOf(cell.Content()).AsString());
                    }
                });
            }
        }
    }
}
