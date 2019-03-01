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

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xive.Test;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Cell.Test
{
    public class MutexCellTest
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

            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                using (var mutexed = new MutexCell(cell))
                {
                    mutexed.Content();
                }
            });
        }

        [Fact]
        public void WorksParallel()
        {
            var cell = new RamCell();
            Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
            {
                var content = Guid.NewGuid().ToString();

                using (var mutexed = new MutexCell(cell))
                {
                    mutexed.Update(new InputOf(content));
                    Assert.Equal(content, new TextOf(mutexed.Content()).AsString());
                }
            });
        }

        [Fact]
        public void WorksWithRamCell()
        {
            var cell = new RamCell();
            using (var mutexed = new MutexCell(cell))
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
                Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
                {
                    using (var cell = new MutexCell(new FileCell(path)))
                    {
                        cell.Update(new InputOf("Ram cell Input"));
                        Assert.Equal("Ram cell Input", new TextOf(cell.Content()).AsString());
                    }
                });
            }
        }
    }
}
