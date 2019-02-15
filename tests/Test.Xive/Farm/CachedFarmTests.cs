using System.Collections.Generic;
using System.Diagnostics;
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

            for(var i=0;i<1000;i++)
            {
                comb.Xocument("some.xml")
                    .Modify(
                        new Directives().Xpath("/some").Add("item")
                    );
            }

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
