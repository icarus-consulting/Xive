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
using Xive.Farm;
using Xive.Hive;
using Xunit;
using Yaapii.Atoms.Scalar;
using Yaapii.Xambly;

namespace Xive.Test.Farm
{
    public sealed class CachedFarmTests
    {
        [Fact]
        public void FillsXmlCache()
        {
            var binMemory = new Dictionary<string, byte[]>();
            var xmlMemory = new Dictionary<string, XNode>();

            var farm =
                new CachedFarm(
                    new RamFarm(),
                    binMemory,
                    xmlMemory
                );

            new Catalog(
                farm.Hive("umbrella-corp")
            ).Create("1024");

            var comb =
                new FirstOf<IHoneyComb>(
                    farm.Hive("umbrella-corp").Combs("@id='1024'")
                )
                .Value();

            comb.Xocument("some.xml")
                .Modify(
                    new Directives().Xpath("/some").Add("item")
                );

            for (int i = 0; i < 10000; i++)
            {
                comb.Xocument("some.xml").Node();
            }
            Assert.Contains("umbrella-corp\\1024\\some.xml", xmlMemory.Keys);
        }

        [Fact]
        public void FillsBinCache()
        {
            var binMemory = new Dictionary<string, byte[]>();
            var xmlMemory = new Dictionary<string, XNode>();

            var farm =
                new CachedFarm(
                    new RamFarm(),
                    binMemory,
                    xmlMemory
                );

            new Catalog(
                farm.Hive("umbrella-corp")
            ).Create("1024");

            var comb =
                new FirstOf<IHoneyComb>(
                    farm.Hive("umbrella-corp").Combs("@id='1024'")
                )
                .Value();

            for (int i = 0; i < 10000; i++)
            {
                comb.Cell("some.xml").Content();
            }
            Assert.Contains("umbrella-corp\\1024\\some.xml", binMemory.Keys);
        }
    }
}
