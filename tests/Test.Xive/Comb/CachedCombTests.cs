using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Comb;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;
using Yaapii.Xml;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Test.Comb
{
    public sealed class CachedCombTests
    {
        [Fact]
        public void ReadsBinaryOnce()
        {
            var binCache = new Dictionary<string, byte[]>();
            int reads = 0;
            var comb =
                new CachedComb(
                    "my-comb",
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
                        (xocName, cell) => new FkXocument()
                    ),
                    binCache,
                    new Dictionary<string, XNode>()
                );

            comb.Cell("adalbert").Content();
            comb.Cell("adalbert").Content();

            Assert.Equal(1, reads);
        }

        [Fact]
        public void ReadsXmlOnce()
        {
            var xmlCache = new Dictionary<string, XNode>();
            int reads = 0;
            var comb =
                new CachedComb(
                    "my-comb",
                    new SimpleComb(
                        "my-comb",
                        cellname => new FkCell(),
                        (xocName, cell) => new FkXocument(() =>
                            {
                                reads++;
                                return new XDocument();
                            }
                        )
                    ),
                    new Dictionary<string, byte[]>(),
                    xmlCache
                );

            comb.Xocument("names.xml").Node();
            comb.Xocument("names.xml").Node();

            Assert.Equal(1, reads);
        }

        [Fact]
        public void FillsBinCache()
        {
            var binCache = new Dictionary<string, byte[]>();
            int reads = 0;
            var comb =
                new CachedComb(
                    "my-comb",
                    new SimpleComb(
                        "my-comb",
                        cellname =>
                            new FkCell(
                                content => { },
                                () =>
                                {
                                    reads++;
                                    return new BytesOf(new InputOf("some data")).AsBytes();
                                }
                            ),
                        (xocName, cell) => new FkXocument()
                    ),
                    binCache,
                    new Dictionary<string, XNode>()
                );

            comb.Cell("my-cell").Content();

            Assert.Equal(
                "some data",
                new TextOf(
                    new InputOf(binCache["my-comb\\my-cell"])
                ).AsString()
            );
        }

        [Fact]
        public void FillsXmlCache()
        {
            var xmlCache = new Dictionary<string, XNode>();
            var comb =
                new CachedComb(
                    "my-comb",
                    new SimpleComb(
                        "my-comb",
                        cellname => new FkCell(),
                        (xocName, cell) => new FkXocument(() =>
                            new XDocument(
                                new XElement("test", new XText("some data"))
                            )
                        )
                    ),
                    new Dictionary<string, byte[]>(),
                    xmlCache
                );

            comb.Cell("my-cell").Content();

            Assert.Equal(
                "some data",
                comb.Xocument("test.xml").Values("/test/text()")[0]
            );
        }

        [Fact]
        public void UpdatesBinaryCache()
        {
            var cache = new Dictionary<string, byte[]>();
            int reads = 0;
            var comb =
                new CachedComb(
                    "my-comb",
                    new SimpleComb(
                        "my-comb",
                        cellname =>
                            new FkCell(
                                content => { },
                                () =>
                                {
                                    reads++;
                                    return new EmptyBytes().AsBytes();
                                }
                            ),
                        (xocName, cell) => new FkXocument()
                    ),
                    cache,
                    new Dictionary<string, XNode>()
                );

            comb.Cell("my-cell").Content();
            comb.Cell("my-cell").Update(new InputOf("New content"));

            Assert.Equal(
                "New content",
                new TextOf(
                    new InputOf(cache["my-comb\\my-cell"])
                ).AsString()
            );
        }

        [Fact]
        public void UpdatesXmlCache()
        {
            var cache = new Dictionary<string, XNode>();
            var contents = new string[] { "<root>old</root>", "<root>new</root>" };
            var index = 0;
            var comb =
                new CachedComb(
                    "my-comb",
                    new SimpleComb(
                        "my-comb",
                        cellname => new FkCell(),
                        (xocName, cell) => new FkXocument(() =>
                            {
                                var result = XDocument.Parse(contents[index]);
                                index++;
                                return result;
                            }
                        )
                    ),
                    new Dictionary<string, byte[]>(),
                    cache
                );

            comb.Xocument("xoc.xml").Node();
            comb.Xocument("xoc.xml").Modify(new EnumerableOf<IDirective>());

            Assert.Equal(
                "new",
                new XMLQuery(cache["my-comb\\xoc.xml"]).Values("/root/text()")[0]
            );
        }
    }
}
