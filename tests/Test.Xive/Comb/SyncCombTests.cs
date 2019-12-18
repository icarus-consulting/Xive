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
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Comb.Test
{
    public class SyncCombTests
    {
        [Fact]
        public void WorksInParallelWithCell()
        {
            var comb =
                new SyncComb(
                    new RamComb("my-comb")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                var content = Guid.NewGuid().ToString();
                using (var cell = comb.Cell("syncCell"))
                {
                    cell.Update(new InputOf(content));
                    Assert.Equal(content, new TextOf(cell.Content()).AsString());
                }
            });
        }

        [Fact]
        public void WorksInParallelWithXocument()
        {
            var comb =
                new SyncComb(
                    new RamComb("my-comb")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                var content = Guid.NewGuid().ToString();
                using (var xoc = comb.Xocument("synced"))
                {
                    xoc.Node();
                    xoc.Modify(new Directives().Xpath("/synced").Set(content));
                    Assert.Equal(content, xoc.Value("/synced/text()", ""));
                }
            });
        }

        [Fact]
        public void DoesNotBlockItself()
        {
            var comb =
                new SyncComb(
                    new RamComb("my-comb")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                var content = Guid.NewGuid().ToString();
                using (var xoc = comb.Xocument("synced"))
                {
                    xoc.Node();
                    xoc.Modify(new Directives().Xpath("/synced").Set(content));
                    Assert.Equal(content, xoc.Value("/synced/text()", ""));
                }
            });
        }

        [Fact]
        public void XocumentAndCellDoNotConflict()
        {
            var comb =
                new SyncComb(
                    new RamComb("my-comb")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                var content = Guid.NewGuid().ToString();
                using (var xoc = comb.Xocument("synced"))
                {
                    xoc.Node();
                    xoc.Modify(new Directives().Xpath("/synced").Set(content));
                    Assert.Equal(content, xoc.Value("/synced/text()", ""));
                }

                content = Guid.NewGuid().ToString();
                using (var cell = comb.Cell("synced"))
                {
                    cell.Update(new InputOf($"<synced>{content}</synced>"));
                    Assert.Equal($"<synced>{content}</synced>", new TextOf(cell.Content()).AsString());
                }
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
                var content = Guid.NewGuid().ToString();
                using (var cell2 = comb2.Cell("syncCell"))
                using (var cell1 = comb1.Cell("syncCell"))
                {
                    cell1.Update(new InputOf(content));
                    cell2.Update(new InputOf(content));
                    Assert.Equal(content, new TextOf(cell1.Content()).AsString());
                }
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

                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    string content = Guid.NewGuid().ToString();
                    using (var cell = comb.Cell("myCell"))
                    {
                        cell.Update(new InputOf(content));
                        Assert.Equal(content, new TextOf(cell.Content()).AsString());
                    }
                });
            }
        }

        [Fact]
        public void WorksWithRamCell()
        {
            var comb =
                new SyncComb(
                    new RamComb("myRamComb")
                );

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                using (var cell = comb.Cell("myCell"))
                {
                    cell.Update(new InputOf("cell content"));
                    Assert.Equal("cell content", new TextOf(cell.Content()).AsString());

                }
            });
        }
    }
}
