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
using Xive.Farm;
using Xive.Hive;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Test.Farm
{
    public class SyncFarmTest
    {
        [Fact]
        public void DeliversHive()
        {
            var farm = new SyncFarm(new RamFarm());
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                Assert.Equal(
                    "my-hive",
                    farm.Hive("my-hive").Name()
                );
            });
        }

        [Fact]
        public void RemembersCatalogChanges()
        {
            var farm = new SyncFarm(new RamFarm());
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                new Catalog(farm.Hive("person")).Create("123");
                Assert.Contains(
                    "123",
                    new Catalog(
                        farm.Hive("person")
                    ).List("@id='123'")
                );
            });
        }

        [Fact]
        public void RemembersCombChanges()
        {
            var farm = new SyncFarm(new RamFarm());
            new Catalog(farm.Hive("person")).Create("123");

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                var comb = new FirstOf<IHoneyComb>(
                    farm.Hive("person").Combs("@id='123'")
                ).Value();

                comb.Cell("address")
                    .Update(new InputOf("the moon"));

                Assert.Equal(
                    "the moon",
                    new TextOf(
                        new InputOf(
                            comb.Cell("address").Content()
                        )
                    ).AsString()
                );
            });
        }
    }
}
