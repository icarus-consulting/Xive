using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic.Content.Test
{
    public sealed class CachedByteContentsTests
    {
        [Fact]
        public void DeliversBytes()
        {
            Assert.Equal(
                0x13,
                new CachedByteContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("a/b/c.dat", new byte[1] { 0x13 })
                    ),
                    8
                ).Bytes("a/b/c.dat", () => new byte[0])[0]
            );
        }

        [Fact]
        public void DoesNotCacheOversized()
        {
            var cache = new ConcurrentDictionary<string, byte[]>();
            var data =
                new CachedByteContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("a/b/c.dat", new byte[2] { 0x13, 0x14 })
                    ),
                    cache,
                    1,
                    new ManyOf()
                ).Bytes("a/b/c.dat", () => throw new ApplicationException("This should not occur"));

            Assert.DoesNotContain("a/b/c.dat", cache.Keys);
        }

        [Fact]
        public void RemovesFromCacheIfOversizedSecondTime()
        {
            var cache = new ConcurrentDictionary<string, byte[]>();
            var contents =
                new CachedByteContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("a/b/c.dat", new byte[1] { 0x13 })
                    ),
                    cache,
                    1,
                    new ManyOf()
                );
            contents.UpdateBytes("a/b/c.dat", new byte[0]);

            Assert.DoesNotContain("a/b/c.dat", cache.Keys);
        }

        [Fact]
        public void DoesNotCacheBlacklisted()
        {
            var cache = new ConcurrentDictionary<string, byte[]>();
            var data =
                new CachedByteContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("a/b/c.dat", new byte[1] { 0x13 })
                    ),
                    cache,
                    1,
                    new ManyOf("*.dat")
                ).Bytes("a/b/c.dat", () => throw new ApplicationException("This should not occur"));

            Assert.DoesNotContain("a/b/c.dat", cache.Keys);
        }

        [Fact]
        public void DeliversXNodeFromOrigin()
        {
            Assert.Equal(
                "<elem>content</elem>",
                new CachedByteContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("a/b/c.xml", new BytesOf(new XDocument(new XElement("elem", "content")).ToString()).AsBytes())
                    ),
                    8
                ).Xml("a/b/c.xml", () => new XDocument()).ToString()
            );
        }

        [Fact]
        public void ReadsBytesFromCacheFirst()
        {
            Assert.Equal(
                0x13,
                new CachedByteContents(
                    new RamContents(),
                    new ConcurrentDictionary<string, byte[]>(
                        new ManyOf<KeyValuePair<string, byte[]>>(
                            new KeyValuePair<string, byte[]>("a/b/c.dat", new byte[1] { 0x13 })
                        )
                    ),
                    8,
                    new ManyOf()
                ).Bytes("a/b/c.dat", () => throw new ApplicationException("This must not occur"))[0]
            );
        }

        [Fact]
        public void CreatesAbsentBytesInOrigin()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            new CachedByteContents(
                new RamContents(mem),
                8
            ).Bytes("a/b/c.dat", () => new byte[1] { 0x13 });

            Assert.Equal(
                0x13,
                mem.GetOrAdd("a/b/c.dat", new byte[0])[0]
            );
        }

        [Fact]
        public void DeliversAbsentBytes()
        {
            Assert.Equal(
                0x13,
                new CachedByteContents(
                    new RamContents(),
                    8
                ).Bytes("a/b/c.dat", () => new byte[1] { 0x13 })[0]
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
        public void DoesNotCacheXml()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            var cached = new CachedByteContents(new RamContents(mem), 8);
            cached.Xml("a/b/c.xml", () => new XDocument(new XElement("first", "one"))).ToString();

            byte[] unused;
            mem.TryRemove("a/b/c.xml", out unused);

            Assert.Throws<ApplicationException>(() =>
                cached.Xml("a/b/c.xml", () => throw new ApplicationException()).ToString()
            );
        }

        [Fact]
        public void UpdatesBytesInCache()
        {
            var mem = new CachedByteContents(new RamContents(), 8);
            mem.Bytes("a/b/c.dat", () => new byte[1] { 0x01 });
            mem.UpdateBytes("a/b/c.dat", new byte[1] { 0x02 });

            Assert.Equal(
                0x02,
                mem.Bytes("a/b/c.dat", () => throw new ApplicationException("This must not occur"))[0]
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
        public void RemovesBytesFromOriginIfEmpty()
        {
            var mem = new ConcurrentDictionary<string, byte[]>();
            var contents = new CachedByteContents(new RamContents(mem), 8);
            contents.Bytes("a/b/c.dat", () => new byte[1] { 0x12 });
            contents.UpdateBytes("a/b/c.dat", new byte[0]);

            Assert.False(
                mem.ContainsKey("a/b/c.xml")
            );
        }

        [Fact]
        public void NormalizesSlashes()
        {
            var mem = new CachedByteContents(new RamContents(), 8);
            mem.Xml(@"childhood\subdir/file", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
            Assert.Equal(
                @"childhood/subdir/file",
                new FirstOf<string>(mem.Knowledge()).Value()
            );
        }

        [Fact]
        public void PreservesCase()
        {
            var mem = new CachedByteContents(new RamContents(), 8);
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
                new CachedByteContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>(@"a/b/c.dat", new byte[1] { 0x13 })
                    ),
                    8
                ).Knowledge()
            );
        }

        [Fact]
        public void UpdatesKnowledge()
        {
            var mem =
                new CachedByteContents(
                    new RamContents(
                        new KeyValuePair<string, byte[]>(@"x", new byte[1] { 0x13 })
                    ),
                    8
                );

            mem.Bytes("x", () => new byte[1] { 0x01 });
            mem.UpdateBytes("x", new byte[0]);

            Assert.Empty(
                mem.Knowledge()
            );
        }
    }
}
