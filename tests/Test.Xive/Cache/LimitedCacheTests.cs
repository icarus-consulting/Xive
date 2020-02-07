using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xive.Hive;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;

namespace Xive.Test.Cache
{
    public sealed class LimitedCacheTests
    {
        [Fact]
        public void DoesNotCacheOversized()
        {
            var cache = new LimitedCache(0, new SimpleMemory());

            Assert.NotEqual(
                cache.Binary("a", () => new MemoryStream(new BytesOf(new Random().Next().ToString()).AsBytes())),
                cache.Binary("a", () => new MemoryStream(new BytesOf(new Random().Next().ToString()).AsBytes()))
            );
        }

        [Fact]
        public void CachesValidSize()
        {
            var cache = new LimitedCache(2048, new SimpleMemory());

            Assert.Equal(
                cache.Binary("a", () => new MemoryStream(new BytesOf(new Random().Next().ToString()).AsBytes())),
                cache.Binary("a", () => new MemoryStream(new BytesOf(new Random().Next().ToString()).AsBytes()))
            );
        }

        [Fact]
        public void Updates()
        {
            var cache = new LimitedCache(2048, new SimpleMemory());
            var bytes = new BytesOf("updated").AsBytes();
            cache.Update("content", new MemoryStream(bytes));

            Assert.Equal(
                bytes,
                cache.Binary("content", () => new MemoryStream(new BytesOf("not updated").AsBytes())).ToArray()
            );
        }

        [Fact]
        public void DoesNotUpdateOversized()
        {
            var cache = new LimitedCache(2, new SimpleMemory());
            var bytes = new BytesOf("updated").AsBytes();
            cache.Update("content", new MemoryStream(bytes));

            Assert.Equal(
                new BytesOf("not updated").AsBytes(),
                cache.Binary("content", () => new MemoryStream(new BytesOf("not updated").AsBytes())).ToArray()
            );
        }
    }
}
