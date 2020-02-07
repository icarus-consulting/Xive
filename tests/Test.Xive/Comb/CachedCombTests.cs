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
using System.Xml.Linq;
using Xive.Hive;
using Xive.Test;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;
using Yaapii.Xml;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Comb.Test
{
    public sealed class CachedCombTests
    {
        //[Fact]
        //public void ReadsBinaryOnce()
        //{
        //    var cache = new SimpleMemory();
        //    int reads = 0;
        //    var comb =
        //        new CachedComb(
        //            new SimpleComb(
        //                "my-comb",
        //                cellname =>
        //                    new FkCell(
        //                        content => { },
        //                        () =>
        //                        {
        //                            reads++;
        //                            return new byte[1];
        //                        }
        //                    ),
        //                (xocName, cell) => new FkXocument()
        //            ),
        //            cache
        //        );

        //    comb.Cell("adalbert/item.txt").Content();
        //    comb.Cell("adalbert\\item.txt").Content();

        //    Assert.Equal(1, reads);
        //}

        //[Fact]
        //public void BlacklistsBinaries()
        //{
        //    var cache = new BlacklistCache("*mister*");
        //    int reads = 0;
        //    var comb =
        //        new CachedComb(
        //            new SimpleComb(
        //                "my-comb",
        //                cellname =>
        //                    new FkCell(
        //                        content => { },
        //                        () =>
        //                        {
        //                            reads++;
        //                            return new byte[1];
        //                        }
        //                    ),
        //                (xocName, cell) => new FkXocument()
        //            ),
        //            cache
        //        );

        //    comb.Cell("hello/mister/adalbert.dat").Content();
        //    comb.Cell("hello\\mister\\adalbert.dat").Content();

        //    Assert.Equal(2, reads);
        //}

        //[Fact]
        //public void BlacklistsXmls()
        //{
        //    var cache = new BlacklistCache("*/*/names.*");
        //    int reads = 0;
        //    var comb =
        //        new CachedComb(
        //            new SimpleComb(
        //                "my-comb",
        //                cellname => new FkCell(),
        //                (xocName, cell) => new FkXocument(() =>
        //                {
        //                    reads++;
        //                    return new XDocument();
        //                }
        //                )
        //            ),
        //            cache
        //        );

        //    comb.Xocument("files/with/names.xml").Node();
        //    comb.Xocument("files\\with\\names.xml").Node();

        //    Assert.Equal(2, reads);
        //}

        //[Fact]
        //public void ReadsXmlOnce()
        //{
        //    var cache = new SimpleMemory();
        //    int reads = 0;
        //    var comb =
        //        new CachedComb(
        //            new SimpleComb(
        //                "my-comb",
        //                cellname => new FkCell(),
        //                (xocName, cell) => new FkXocument(() =>
        //                    {
        //                        reads++;
        //                        return new XDocument();
        //                    }
        //                )
        //            ),
        //            cache
        //        );

        //    comb.Xocument("my/names.xml").Node();
        //    comb.Xocument("my\\names.xml").Node();

        //    Assert.Equal(1, reads);
        //}

        //[Fact]
        //public void FillsBinCache()
        //{
        //    var cache = new SimpleMemory();
        //    int reads = 0;
        //    var comb =
        //        new CachedComb(
        //            new SimpleComb(
        //                "my-comb",
        //                cellname =>
        //                    new FkCell(
        //                        content => { },
        //                        () =>
        //                        {
        //                            reads++;
        //                            return new BytesOf(new InputOf("some data")).AsBytes();
        //                        }
        //                    ),
        //                (xocName, cell) => new FkXocument()
        //            ),
        //            cache
        //        );

        //    comb.Cell("my-cell").Content();

        //    Assert.Equal(
        //        "some data",
        //        new TextOf(
        //            new InputOf(cache.Binary("my-comb\\my-cell", () => new MemoryStream()))
        //        ).AsString()
        //    );
        //}

        //[Fact]
        //public void FillsXmlCache()
        //{
        //    var cache = new SimpleMemory();
        //    var comb =
        //        new CachedComb(
        //            new SimpleComb(
        //                "my-comb",
        //                cellname => new FkCell(),
        //                (xocName, cell) => 
        //                new FkXocument(() =>
        //                    new XDocument(
        //                        new XElement("test", new XText("some data"))
        //                    )
        //                )
        //            ),
        //            cache
        //        );

        //    comb.Xocument("test\\slash").Node();

        //    Assert.Equal(
        //        "some data",
        //        new XMLCursor(
        //            cache.Xml(
        //                "my-comb/test/slash", 
        //                () => new XElement("not-this")
        //            )
        //        ).Values("/test/text()")[0]
        //    );
        //}

        //[Fact]
        //public void UpdatesBinaryCache()
        //{
        //    var cache = new SimpleMemory();
        //    int reads = 0;
        //    var comb =
        //        new CachedComb(
        //            new SimpleComb(
        //                "my-comb",
        //                cellname =>
        //                    new FkCell(
        //                        content => { },
        //                        () =>
        //                        {
        //                            reads++;
        //                            return new byte[0];
        //                        }
        //                    ),
        //                (xocName, cell) => new FkXocument()
        //            ),
        //            cache
        //        );

        //    comb.Cell("my/cell").Content();
        //    comb.Cell("my\\cell").Update(new InputOf("New content"));

        //    Assert.Equal(
        //        "New content",
        //        new TextOf(
        //            new InputOf(cache.Binary("my-comb/my/cell", () => new MemoryStream()))
        //        ).AsString()
        //    );
        //}

        //[Fact]
        //public void UpdatesXmlCache()
        //{
        //    var cache = new SimpleMemory();
        //    var contents = new string[] { "<root>old</root>", "<root>new</root>" };
        //    var index = 0;
        //    var comb =
        //        new CachedComb(
        //            new SimpleComb(
        //                "my-comb",
        //                cellname => new FkCell(),
        //                (xocName, cell) => new FkXocument(() =>
        //                    {
        //                        var result = XDocument.Parse(contents[index]);
        //                        index++;
        //                        return result;
        //                    }
        //                )
        //            ),
        //            cache
        //        );

        //    comb.Xocument("xockt/xoc.xml").Node();
        //    comb.Xocument("xockt\\xoc.xml").Modify(new EnumerableOf<IDirective>());

        //    Assert.Equal(
        //        "new",
        //        new XMLCursor(
        //            cache.Xml("my-comb/xockt/xoc.xml", () => new XElement("not-this"))
        //        ).Values("/root/text()")[0]
        //    );
        //}
    }
}
