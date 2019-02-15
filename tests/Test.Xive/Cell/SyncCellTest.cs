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
