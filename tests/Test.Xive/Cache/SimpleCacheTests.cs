using System;
using System.IO;
using System.Xml.Linq;
using Xive.Hive;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Text;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Test.Cache
{
    public sealed class SimpleCacheTests
    {
        [Fact]
        public void CachesBinaries()
        {
            var cache = new SimpleCache();

            Assert.Equal(
                cache.Binary(
                    "test",
                    () => new MemoryStream(
                        new BytesOf(
                            new Random().Next().ToString()
                        ).AsBytes())
                    ),
                cache.Binary(
                    "test",
                    () => new MemoryStream()
                )
            );
        }

        [Fact]
        public void CachesXml()
        {
            var cache = new SimpleCache();

            Assert.Equal(
                cache.Xml(
                    "test",
                    () => new XElement("node", new XText(new Random().Next().ToString()))
                ).ToString(),
                cache.Xml(
                    "test",
                    () => new XElement("node", "Not the number from above")
                ).ToString()
            );
        }

        [Fact]
        public void UpdatesCacheWithXNode()
        {
            var cache = new SimpleCache();
            var node = new XElement("node", new XText("someNiceNode"));
            cache.Update(
                "test",
                node
            );
            var second =
                cache.Xml(
                    "test",
                    () => new XElement("node", new XText(new Random().Next().ToString()))
                ).ToString();

            Assert.Equal(node.ToString(), second);
        }

        [Fact]
        public void UpdatesCacheWithBinary()
        {
            var cache = new SimpleCache();
            var binary = new MemoryStream(new BytesOf("nice binary things").AsBytes());
            cache.Update(
                "test",
                binary
            );
            var second =
                cache.Binary(
                    "test",
                    () => new MemoryStream(new BytesOf("some other things").AsBytes())
                );

            Assert.Equal("nice binary things", new TextOf(second).AsString());
        }

        [Fact]
        public void RejectsCrossUsageOfXML()
        {
            var cache = new SimpleCache();
            cache.Xml("test", () => new XElement("a"));
            Assert.Throws<InvalidOperationException>(() => 
                cache.Binary("test", () => new MemoryStream())
            );
        }

        [Fact]
        public void RejectsCrossUsageOfBinary()
        {
            var cache = new SimpleCache();
            cache.Binary("test", () => new MemoryStream());
            Assert.Throws<InvalidOperationException>(() =>
                cache.Xml("test", () => new XElement("a"))
            );
        }

        [Fact]
        public void KnowsWhatItHas()
        {
            var cache = new SimpleCache();
            cache.Binary("Cheezeburger", () => new MemoryStream());
            Assert.True(cache.Has("Cheezeburger"));
        }
    }
}
