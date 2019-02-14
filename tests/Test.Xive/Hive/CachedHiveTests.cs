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
                    "phonebook",
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
    }
}
