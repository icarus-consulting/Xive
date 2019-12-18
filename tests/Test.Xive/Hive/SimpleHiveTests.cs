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

using System.Collections.Generic;
using System.IO;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Xive.Comb;

namespace Xive.Hive.Test
{
    public sealed class SimpleHiveTests
    {
        [Fact]
        public void DeliversHQ()
        {
            var mem = new Dictionary<string, MemoryStream>();
            var hq =
                new SimpleHive(
                    "person", 
                    comb => new RamComb(comb, mem)
                ).HQ();

            Assert.Equal($"person/HQ", hq.Name());
        }

        [Fact]
        public void RemembersHQ()
        {
            var mem = new Dictionary<string, MemoryStream>();
            var hq =
                new SimpleHive(
                    "person",
                    comb => new RamComb(comb, mem)
                ).HQ();

            hq.Cell("my-cell").Update(new InputOf("larva"));

            Assert.Equal(
                "larva",
                new TextOf(
                    new InputOf(
                        hq.Cell("my-cell").Content()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void DeliversComb()
        {
            var mem = new Dictionary<string, MemoryStream>();
            var hq =
                new SimpleHive(
                    "person",
                    comb => new RamComb(comb, mem)
                ).HQ();

            var catalog = new MutexCatalog("person", hq);
            catalog.Create("Alfred");

            Assert.Contains("Alfred", catalog.List("@id='Alfred'"));
        }
    }
}
