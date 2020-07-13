using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic.Content.Test
{
    public sealed class CachedXmlContentsTests
    {
        [Fact]
        public void DeliversXNode()
        {
            Assert.Equal(
                "<elem>content</elem>",
                new CachedXmlContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("a/b/c.xml", new BytesOf(new XDocument(new XElement("elem", "content")).ToString()).AsBytes())
                    )
                ).Xml("a/b/c.xml", () => new XDocument()).ToString()
            );
        }

        [Fact]
        public void CachesOnRead()
        {
            var cache = new ConcurrentDictionary<string, XNode>();
            var data =
                new CachedXmlContents(
                    new RamContents(),
                    cache,
                    new ManyOf()
                );

            data.Xml("a/b/c.xml", () => new XDocument(new XElement("more", "than 8 chars")));

            Assert.Contains("a/b/c.xml", cache.Keys);
        }

        [Fact]
        public void CachesOnUpdate()
        {
            var cache = new ConcurrentDictionary<string, XNode>();
            var data =
                new CachedXmlContents(
                    new RamContents(),
                    cache,
                    new ManyOf()
                );

            data.UpdateXml("a/b/c.xml", new XDocument(new XElement("more", @"contents for everyone \o/")));

            Assert.Contains("a/b/c.xml", cache.Keys);
        }

        [Fact]
        public void DoesNotCacheBlacklisted()
        {
            var cache = new ConcurrentDictionary<string, XNode>();
            var data =
                new CachedXmlContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>(
                            "a/b/c.xml", 
                            new BytesOf(
                                new XDocument(
                                    new XElement("my", "document")
                                ).ToString()
                            ).AsBytes()
                        )
                    ),
                    cache,
                    new ManyOf("*.xml")
                ).Bytes("a/b/c.xml", () => throw new ApplicationException("This should not occur"));

            Assert.DoesNotContain("a/b/c.xml", cache.Keys);
        }

        [Fact]
        public void DeliversXNodeFromCache()
        {
            Assert.Equal(
                "<test>rest</test>",
                new CachedXmlContents(
                    new RamContents(),
                    new ConcurrentDictionary<string, XNode>(
                        new ManyOf<KeyValuePair<string, XNode>>(
                            new KeyValuePair<string, XNode>("a/b/c.xml", new XDocument(new XElement("test", "rest")))
                        )
                    ),
                    new ManyOf()
                ).Xml("a/b/c.xml", () => throw new ApplicationException("This must not occur"))
                .ToString(SaveOptions.DisableFormatting)
            );
        }

        [Fact]
        public void ReadsXNodeFromCacheFirst()
        {
            Assert.Equal(
                "<test>rest</test>",
                new CachedXmlContents(
                    new RamContents(),
                        new ConcurrentDictionary<string, XNode>(
                        new ManyOf<KeyValuePair<string, XNode>>(
                            new KeyValuePair<string, XNode>("a/b/c.xml", new XDocument(new XElement("test", "rest")))
                        )
                    ),
                    new ManyOf()
                ).Xml("a/b/c.xml", () => throw new ApplicationException("This must not occur")).ToString()
            );
        }

        [Fact]
        public void CreatesAbsentXNodeInOrigin()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            new CachedXmlContents(
                new RamContents(mem)
            ).Xml("a/b/c.xml", () => new XDocument(new XElement("test", "best")));

            Assert.Equal(
                "<test>best</test>",
                new TextOf(mem.GetOrAdd("a/b/c.xml", (n) => new byte[0])).AsString()
            );
        }

        [Fact]
        public void CreatesAbsentBytesInOrigin()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            new CachedXmlContents(
                new RamContents(mem)
            ).Xml("a/b/c.xml", () => new XDocument(new XElement("test", "everything")));

            Assert.Equal(
                new XNodeBytes(new XDocument(new XElement("test", "everything"))).AsBytes(),
                mem.GetOrAdd("a/b/c.xml", new byte[0])
            );
        }

        [Fact]
        public void DeliversAbsentXNode()
        {
            Assert.Equal(
                "<elem>content</elem>",
                new CachedXmlContents(
                    new RamContents()
                ).Xml("a/b/c.xml", () => new XDocument(new XElement("elem", "content"))).ToString()
            );
        }

        [Fact]
        public void RemembersAbsentXmlAfterFirstRead()
        {
            var mem = new CachedByteContents(new RamContents(), 1024);
            mem.Xml("a/b/c.xml", () => new XDocument(new XElement("elem", "content")));

            Assert.Equal(
                "<elem>content</elem>",
                mem.Xml("a/b/c.xml", () => new XDocument()).ToString()
            );
        }

        [Fact]
        public void DoesNotCacheBytes()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            var cached = new CachedByteContents(new RamContents(mem), 1024);
            cached.Bytes("a/b/c.dat", () => new byte[0]).ToString();

            byte[] unused;
            mem.TryRemove("a/b/c.dat", out unused);

            Assert.Throws<ApplicationException>(() =>
                cached.Xml("a/b/c.dat", () => throw new ApplicationException()).ToString()
            );
        }

        [Fact]
        public void UpdatesXmlInCache()
        {
            var mem = new CachedXmlContents(new RamContents());
            mem.Xml("a/b/c.xml", () => new XDocument(new XElement("first", "read")));
            mem.UpdateXml("a/b/c.xml", new XDocument(new XElement("updated", "read")));

            Assert.Equal(
                "<updated>read</updated>",
                mem.Xml("a/b/c.xml", () => throw new ApplicationException("This must not occur")).ToString()
            );
        }

        [Fact]
        public void RemovesBytesFromCacheIfEmpty()
        {
            var bytesCache = new ConcurrentDictionary<string, byte[]>();
            var contents = new CachedByteContents(new RamContents(), bytesCache, 8, new ManyOf());
            contents.Bytes("a/b/c.dat", () => new byte[1] { 0x12 });
            contents.UpdateBytes("a/b/c.dat", new byte[0]);

            Assert.False(
                bytesCache.ContainsKey("a/b/c.xml")
            );
        }

        [Fact]
        public void RemovesXmlFromOriginIfEmpty()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            var contents = new CachedXmlContents(new RamContents(mem));
            contents.Xml("a/b/c.xml", () => new XDocument(new XElement("first", "read")));
            contents.UpdateXml("a/b/c.xml", new XDocument());

            Assert.False(
                mem.ContainsKey("a/b/c.xml")
            );
        }

        [Fact]
        public void NormalizesSlashes()
        {
            var mem = new CachedXmlContents(new RamContents());
            mem.Xml(@"childhood\subdir/file", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
            Assert.Equal(
                @"childhood/subdir/file",
                new FirstOf<string>(mem.Knowledge()).Value()
            );
        }

        [Fact]
        public void PreservesCase()
        {
            var mem = new CachedXmlContents(new RamContents());
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
                "a/b/c.xml",
                new CachedXmlContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>(
                            @"a/b/c.xml", 
                            new BytesOf(new XDocument(new XElement("not", "empty")).ToString()).AsBytes()
                        )
                    )
                ).Knowledge()
            );
        }
    }
}
