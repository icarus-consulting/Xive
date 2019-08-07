using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Xive.Hive;
using Xunit;

namespace Xive.Test.Cache
{
    public sealed class BlacklistCacheTests
    {
        [Fact]
        public void CachesNonListed()
        {
            var cache = new BlacklistCache("a/*.*");

            var first = 
                cache.Xml(
                    "b/some.xml", 
                    () => 
                    new XElement("a-node",
                        new XText(
                            new Random().Next().ToString())
                        )
                    ).ToString();
            var second =
                cache.Xml(
                    "b/some.xml",
                    () =>
                    new XElement("a-node",
                        new XText(
                            new Random().Next().ToString())
                        )
                    ).ToString();

            Assert.Equal(first, second);
        }

        [Fact]
        public void DoesNotCacheBlacklisted()
        {
            var cache = new BlacklistCache("a/*.*");

            var first =
                cache.Xml(
                    "a/some.xml",
                    () =>
                    new XElement("a-node",
                        new XText(
                            new Random().Next().ToString())
                        )
                    ).ToString();
            var second =
                cache.Xml(
                    "a/some.xml",
                    () =>
                    new XElement("a-node",
                        new XText(
                            new Random().Next().ToString())
                        )
                    ).ToString();

            Assert.NotEqual(first, second);
        }
    }
}
