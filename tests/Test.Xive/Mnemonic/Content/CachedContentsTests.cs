using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;
using Xive.Mnemonic.Cache;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic.Content.Test
{
    public sealed class CachedContentsTests
    {
        [Fact]
        public void DeliversBytes()
        {
            Assert.Equal(
                0x13,
                new CachedContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("a/b/c.dat", new byte[1] { 0x13 })
                    )
                ).Bytes("a/b/c.dat", () => new byte[0])[0]
            );
        }

        [Fact]
        public void UpdatesOriginIfIgnored()
        {
            var contents = new RamContents();
            new CachedContents(
                contents,
                new ManyOf<string>("a/*"),
                Int64.MaxValue
            ).UpdateBytes(
                "a/b/c.dat",
                new byte[1] { 0x13 }
            );

            Assert.Equal(
                0x13,
                contents.Bytes("a/b/c.dat", () => new byte[0])[0]
            );
        }

        [Fact]
        public void DeliversXNodeFromBytes()
        {
            Assert.Equal(
                "<elem>content</elem>",
                new CachedContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("a/b/c.xml", new BytesOf(new XDocument(new XElement("elem", "content")).ToString()).AsBytes())
                    )
                ).Xml("a/b/c.xml", () => new XDocument()).ToString()
            );
        }

        [Fact]
        public void UpdatesXNodeIfIgnored()
        {
            var contents = new RamContents();
            new CachedContents(
                contents,
                new ManyOf<string>("a/*"),
                Int64.MaxValue
            ).UpdateXml(
                "a/b/c.dat",
                new XDocument(new XElement("elem", "content"))
            );

            Assert.Equal(
                "<elem>content</elem>",
                contents.Xml("a/b/c.dat", () => new XDocument()).ToString()
            );
        }

        [Fact]
        public void ReadsBytesFromCacheFirst()
        {
            Assert.Equal(
                0x13,
                new CachedContents(
                    new RamContents(),
                    new BytesCache(
                        new KeyValuePair<string, byte[]>("a/b/c.dat", new byte[1] { 0x13 })
                    ),
                    new XmlCache()
                ).Bytes("a/b/c.dat", () => throw new ApplicationException("This must not occur"))[0]
            );
        }

        [Fact]
        public void ReadsXNodeFromCacheFirst()
        {
            Assert.Equal(
                "<test>rest</test>",
                new CachedContents(
                    new RamContents(),
                    new BytesCache(),
                    new XmlCache(
                        new KeyValuePair<string, XNode>("a/b/c.xml", new XDocument(new XElement("test", "rest")))
                    )
                ).Xml("a/b/c.xml", () => throw new ApplicationException("This must not occur"))
                .ToString(SaveOptions.DisableFormatting)
            );
        }

        [Fact]
        public void CreatesAbsentBytesInOrigin()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            new CachedContents(
                new RamContents(mem)
            ).Bytes("a/b/c.dat", () => new byte[1] { 0x13 });

            Assert.Equal(
                0x13,
                mem.GetOrAdd("a/b/c.dat", new byte[0])[0]
            );
        }

        [Fact]
        public void CreatesAbsentXNodeInOrigin()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            new CachedContents(
                new RamContents(mem)
            ).Xml("a/b/c.xml", () => new XDocument(new XElement("test", "best")));

            Assert.Equal(
                "<test>best</test>",
                new TextOf(mem.GetOrAdd("a/b/c.xml", (n) => new byte[0])).AsString()
            );
        }

        [Fact]
        public void DeliversAbsentBytes()
        {
            Assert.Equal(
                0x13,
                new CachedContents(
                    new RamContents()
                ).Bytes("a/b/c.dat", () => new byte[1] { 0x13 })[0]
            );
        }

        [Fact]
        public void DeliversAbsentXNode()
        {
            Assert.Equal(
                "<elem>content</elem>",
                new CachedContents(
                    new RamContents()
                ).Xml("a/b/c.xml", () => new XDocument(new XElement("elem", "content"))).ToString()
            );
        }

        [Fact]
        public void RemembersAbsentBytesAfterFirstRead()
        {
            var mem = new CachedContents(new RamContents());
            mem.Bytes("a/b/c.dat", () => new byte[1] { 0x13 }).ToString();

            Assert.Equal(
                new byte[1] { 0x13 },
                mem.Bytes("a/b/c.dat", () => new byte[0])
            );
        }

        [Fact]
        public void RemembersAbsentXmlBytesAfterFirstRead()
        {
            var mem = new CachedContents(new RamContents());
            mem.Xml("a/b/c.xml", () => new XDocument(new XElement("elem", "content"))).ToString();

            Assert.Equal(
                "<elem>content</elem>",
                new TextOf(mem.Bytes("a/b/c.xml", () => new byte[0])).AsString()
            );
        }

        [Fact]
        public void UpdatesBytesInCache()
        {
            var mem = new CachedContents(new RamContents());
            mem.Bytes("a/b/c.dat", () => new byte[1] { 0x01 });
            mem.UpdateBytes("a/b/c.dat", new byte[1] { 0x02 });

            Assert.Equal(
                0x02,
                mem.Bytes("a/b/c.dat", () => throw new ApplicationException("This must not occur"))[0]
            );
        }

        [Fact]
        public void UpdatesXNodeInCache()
        {
            var mem = new CachedContents(new RamContents());
            mem.Xml("a/b/c.xml", () => new XDocument(new XElement("first", "time")));
            mem.UpdateXml("a/b/c.xml", new XDocument(new XElement("second", "time")));

            Assert.Equal(
                "<second>time</second>",
                mem.Xml("a/b/c.xml", () => throw new ApplicationException("This must not occur"))
                .ToString(SaveOptions.DisableFormatting)
            );
        }

        [Fact]
        public void RemovesBytesFromOriginIfEmpty()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            var contents = new CachedContents(new RamContents(mem));
            contents.Bytes("a/b/c.dat", () => new byte[1] { 0x12 });
            contents.UpdateBytes("a/b/c.dat", new byte[0]);

            Assert.False(
                mem.ContainsKey("a/b/c.xml")
            );
        }


        [Fact]
        public void RemovesBytesFromCacheIfEmpty()
        {
            var removed = string.Empty;
            var contents =
                new CachedContents(
                    new RamContents(),
                    new FkCache<byte[]>(name => removed = name),
                    new XmlCache()
                );
            contents.UpdateBytes("a/b/c.dat", new byte[0]);

            Assert.Equal(
                "a/b/c.dat",
                removed
            );
        }

        [Fact]
        public void RemovesXNodeFromCacheIfEmpty()
        {
            var removed = string.Empty;
            var contents = 
                new CachedContents(
                    new RamContents(),
                    new BytesCache(),
                    new FkCache<XNode>(
                        name => removed = name
                    )
                );

            contents.UpdateXml("a/b/c.xml", new XDocument());

            Assert.Equal(
                "a/b/c.xml",
                removed
            );
        }

        [Fact]
        public void RemovesXNodeFromOriginIfEmpty()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            var contents = new CachedContents(new RamContents(mem));
            contents.Xml("a/b/c.xml", () => new XDocument(new XElement("first", "time")));
            contents.UpdateXml("a/b/c.xml", new XDocument());

            Assert.False(
                mem.ContainsKey("a/b/c.xml")
            );
        }

        [Fact]
        public void NormalizesSlashes()
        {
            var mem = new CachedContents(new RamContents());
            mem.Xml(@"childhood\subdir/file", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
            Assert.Equal(
                @"childhood/subdir/file",
                new FirstOf<string>(mem.Knowledge()).Value()
            );
        }

        [Fact]
        public void PreservesCase()
        {
            var mem = new CachedContents(new RamContents());
            mem.Xml(@"BIG\subdir/file", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
            Assert.Equal(
                "BIG/subdir/file",
                new FirstOf<string>(mem.Knowledge()).Value()
            );
        }

        [Fact]
        public void DeliversKnowledge()
        {
            Assert.Contains(
                "a/b/c.dat",
                new CachedContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>(@"a/b/c.dat", new byte[1] { 0x13 })
                    )
                ).Knowledge()
            );
        }

        [Fact]
        public void UpdatesKnowledge()
        {
            var mem =
                new CachedContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>(@"x", new byte[1] { 0x13 })
                    )
                );

            mem.Bytes("x", () => new byte[1] { 0x01 });
            mem.UpdateBytes("x", new byte[0]);

            Assert.Empty(
                mem.Knowledge()
            );
        }

        [Fact]
        public void WorksWithExceedingBytes()
        {
            var mem =
                new CachedContents(
                    new RamContents(),
                    new ManyOf<string>(),
                    maxSize: 110
                );

            mem.UpdateBytes("x", new byte[80]);
            mem.UpdateBytes("x", new byte[220]);

            Assert.Equal(
                220,
                mem.Bytes("x", ()=>new byte[0]).Length
            );
        }
    }
}
