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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Test.Yaapii.Xive;
using Xive.Mnemonic.Sync;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public class SyncHiveTest
    {
        [Fact]
        public void DeliversNameInParallel()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    Assert.Equal("product", hive.Scope());
                });
            }
        }

        [Fact]
        public void WorksWithFileHive()
        {
            using (var dir = new TempDirectory())
            {
                var valve = new LocalSyncPipe();
                var hive = new FileHive(dir.Value().FullName, "product");
                hive.Catalog().Add("2CV");
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    hive.Comb("2CV");
                    hive.Scope();
                    hive.HQ();
                });
            }
        }

        [Fact]
        public void UpdatesCellsInParallel()
        {
            var hive = new RamHive("product");
            var comb = "Dr.Robotic";
            var index = -1;
            var contents = new ConcurrentDictionary<string, string>();
            new ParallelFunc(() =>
                {
                    var id = "Item_" + ++index;
                    var content = Guid.NewGuid().ToString();
                    hive.Catalog().Add(comb);
                    hive.Comb(comb).Cell(id).Update(new InputOf(content));

                    contents.AddOrUpdate(id, content, (a, b) => content);
                    return true;
                },
                Environment.ProcessorCount << 4,
                10000
            ).Invoke();

            Parallel.ForEach(contents.Keys, (record) =>
            {
                Assert.Equal(
                    contents[record],
                    new TextOf(hive.Comb(comb).Cell(record).Content()).AsString()
                );
            });
        }

        [Fact]
        public void UpdatesComplexWithFileHive()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");

                var machine = "Dr.Robotic";
                new ParallelFunc(() =>
                {
                    var id = Guid.NewGuid().ToString();
                    hive.Shifted("to-the-left").Catalog().Add(machine);

                    var content = Guid.NewGuid().ToString();
                    hive.Shifted("to-the-left")
                        .Comb(machine)
                        .Cell(id)
                        .Update(new InputOf(content));

                    Assert.Equal(
                        content,
                        new TextOf(
                            hive.Shifted("to-the-left")
                                .Comb(machine)
                                .Cell(id)
                                .Content()
                        ).AsString()
                    );
                    return true;
                },
                256,
                10000
            ).Invoke();
            }
        }

        [Fact]
        public void WorksParallelWithRamHive()
        {
            var hive = new RamHive("product");
            var machine = "Dr.Robotic";
            new ParallelFunc(() =>
            {
                var id = Guid.NewGuid().ToString();
                hive.Shifted("to-the-left").Catalog().Add(machine);

                var content = Guid.NewGuid().ToString();
                hive.Shifted("to-the-left")
                    .Comb(machine)
                    .Cell(id)
                    .Update(new InputOf(content));

                Assert.Equal(
                    content,
                    new TextOf(
                        hive.Shifted("to-the-left")
                            .Comb(machine)
                            .Cell(id)
                            .Content()
                    ).AsString()
                );
                return true;
            },
            256,
            10000
        ).Invoke();
        }
    }
}