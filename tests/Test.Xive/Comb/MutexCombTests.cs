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
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Comb.Test
{
    public class MutexCombTests
    {
        [Fact]
        public void WorksInParallel()
        {
            var comb = 
                new MutexComb(
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
            var comb1 = new MutexComb(new RamComb("my-comb"));
            var comb2 = new MutexComb(new RamComb("my-comb"));
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
                    new MutexComb(
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
                new MutexComb(
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
