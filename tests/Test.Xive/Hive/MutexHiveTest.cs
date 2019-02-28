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

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public class MutexHiveTest
    {
        [Fact]
        public void CreatesCatalogInParallel()
        {
            var hive = new MutexHive(new RamHive("testRamHive"));

            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                var catalog = new Catalog(hive);
                catalog.Create("123");
            });
        }

        [Fact]
        public void DeliversCombsInParallel()
        {
            var hive = new MutexHive(new RamHive("product"));
            new Catalog(hive).Create("2CV");

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.Combs("@id='2CV'");
            });
            
        }

        [Fact]
        public void Shifts()
        {
            var hive =
                new MutexHive(
                    new RamHive("person")
                );

            Assert.Equal(
                "left",
                hive.Shifted("left").Scope()
            );
        }

        [Fact]
        public void DeliversHQInParallelAfterShift()
        {
            var hive =
                new MutexHive(
                    new RamHive("person")
                ).Shifted("still-parallel");

            var first = true;
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                if(!first)
                {
                    new Catalog(hive).Remove("X");
                    first = false;
                }
                new Catalog(hive).Create("X");
            });
        }

        [Fact]
        public void DeliversHQInParallel()
        {
            var hive =
                new MutexHive(
                    new RamHive("person")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.HQ();
            });
        }

        [Fact]
        public void DeliversNameInParallel()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new MutexHive(new FileHive("product", dir.Value().FullName));
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    hive.Scope();
                });
            }
        }

        [Fact]
        public void WorksWithFileHive()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new MutexHive(new FileHive("product", dir.Value().FullName));
                var catalog = new Catalog(hive);
                catalog.Create("2CV");
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    hive.Combs("@id='2CV'");
                    hive.Scope();
                    hive.HQ();
                });
            }
        }
    }
}