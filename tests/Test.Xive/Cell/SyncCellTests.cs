//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Cell.Test
{
    public class SyncCellTest
    {
        [Fact]
        public void ParallelAccessWorks()
        {
            var content = new BytesOf("Works").AsBytes();
            var accesses = 0;
            var cell =
                new FkCell(
                    (update) => { },
                    () =>
                    {
                        accesses++;
                        Assert.Equal(1, accesses);
                        accesses--;
                        return content;
                    }
                );

            var valve = new LocalSyncPipe();
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                Assert.Equal(
                    "Works",
                    new TextOf(
                        new SyncCell(cell, valve).Content()
                    ).AsString()
                );
            });
        }

        [Fact]
        public void DoesNotBlockItself()
        {
            var cell = new RamCell();
            var gate = new LocalSyncPipe();
            Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
            {
                var content = "ABC";
                var first = new SyncCell(cell, gate);
                first.Update(new InputOf(content));
                var again = new SyncCell(cell, gate);
                again.Update(new InputOf(content));
                var andAgain = new SyncCell(cell, gate);
                andAgain.Update(new InputOf(content));
                System.Threading.Thread.Sleep(1);
                Assert.Equal(
                    "ABC ABC ABC",
                    $"{new TextOf(first.Content()).AsString()} {new TextOf(again.Content()).AsString()} {new TextOf(andAgain.Content()).AsString()}"
                );
            });
        }

        [Fact]
        public void WorksWithRamCell()
        {
            var cell = new RamCell();
            var gate = new LocalSyncPipe();
            var synced = new SyncCell(cell, gate);
            synced.Update(new InputOf("its so hot outside"));
            Assert.Equal(
                "its so hot outside",
                new TextOf(synced.Content()).AsString()
            );
        }

        [Fact]
        public void WorksWithFileCell()
        {
            using (var file = new TempFile())
            {
                var path = file.Value();
                var gate = new LocalSyncPipe();
                Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
                {
                    var cell = new SyncCell(new FileCell(path), gate);
                    cell.Update(new InputOf("File cell Input"));
                    Assert.Equal("File cell Input", new TextOf(cell.Content()).AsString());
                });
            }
        }
    }
}
