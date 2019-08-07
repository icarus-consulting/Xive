using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xive.Hive;
using Xunit;
using Yaapii.Atoms.Bytes;

namespace Xive.Test.Cache
{
    public sealed class LimitedCacheTests
    {
        [Fact]
        public void DoesNotCacheOversized()
        {
            var cache = new LimitedCache(0, new SimpleCache());

            Assert.NotEqual(
                cache.Binary("a", () => new MemoryStream(new BytesOf(new Random().Next().ToString()).AsBytes())),
                cache.Binary("a", () => new MemoryStream(new BytesOf(new Random().Next().ToString()).AsBytes()))
            );
        }

        [Fact]
        public void CachesValidSize()
        {
            var cache = new LimitedCache(2048, new SimpleCache());

            Assert.Equal(
                cache.Binary("a", () => new MemoryStream(new BytesOf(new Random().Next().ToString()).AsBytes())),
                cache.Binary("a", () => new MemoryStream(new BytesOf(new Random().Next().ToString()).AsBytes()))
            );
        }
    }
}
