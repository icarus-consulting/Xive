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
using System.Xml.Linq;
using Xive.Comb;
using Xive.Test;
using Xive.Xocument;
using Xunit;
using Yaapii.Atoms.Scalar;

namespace Xive.Hive.Test
{
    public sealed class CachedHiveTests
    {
        [Fact]
        public void ReadsBinaryOnce()
        {
            var binCache = new Dictionary<string, byte[]>();
            var xmlMemory = new Dictionary<string, XNode>();
            int reads = 0;
            var hive =
                new CachedHive(
                    new SimpleHive("phonebook",
                        combName =>
                        new SimpleComb(
                            "my-comb",
                            cellname =>
                                new FkCell(
                                    content => { },
                                    () =>
                                    {
                                        reads++;
                                        return new byte[0];
                                    }
                                ),
                            (x, c) => new CachedXocument(x, new SimpleXocument("catalog"), xmlMemory)
                        )
                    ),
                    binCache,
                    xmlMemory
                );

            new Catalog(hive).Create("123");

            var cell =
                new FirstOf<IHoneyComb>(
                    hive.Combs("@id='123'")
                )
                .Value()
                .Cell("adalbert");

            cell.Content();
            cell.Content();

            Assert.Equal(1, reads);
        }

        [Fact]
        public void ShiftIncludesCache()
        {
            var binCache = new Dictionary<string, byte[]>();
            var xmlMemory = new Dictionary<string, XNode>();

            var hive =
                new CachedHive(
                    new RamHive(),
                    binCache,
                    xmlMemory
                );

            new Catalog(hive.Shifted("A")).Create("something");
            new Catalog(hive.Shifted("B")).Create("another thing");

            Assert.Contains(@"B\HQ\catalog.xml", xmlMemory.Keys);
        }
    }
}
