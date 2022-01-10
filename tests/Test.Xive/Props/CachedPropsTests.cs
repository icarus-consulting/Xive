using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Yaapii.Atoms.Enumerable;

namespace Xive.Props.Test
{
    public sealed class CachedPropsTests
    {
        [Fact]
        public void ListCacheKeys()
        {
            var cache = new ConcurrentDictionary<string, string[]>();
            cache["one"] = new string[] { "test1" };
            cache["two"] = new string[] { "test1" };

            Assert.Equal(
                new ManyOf("one", "two"),
                new Sorted<string>(
                    new CachedProps(
                        new RamProps(),
                        cache
                    ).Names()
                )
            );
        }

        [Fact]
        public void Refines()
        {
            var cache = new ConcurrentDictionary<string, string[]>();
            var props = new CachedProps(new RamProps(), cache);

            props.Refined("test", "value");

            Assert.Equal(
                "value",
                cache["test"][0]
            );
        }

        [Fact]
        public void RefinesOrigin()
        {
            var cache = new ConcurrentDictionary<string, string[]>();
            var origin = new RamProps();
            var props = new CachedProps(origin, cache);

            props.Refined("test", "value");

            Assert.Equal(
                "value",
                origin.Value("test")
            );
        }

        [Fact]
        public void Caches()
        {
            var cache = new ConcurrentDictionary<string, string[]>();
            var origin = new RamProps();
            var props = new CachedProps(origin, cache);

            cache["test"] = new string[] { "something" };
            origin.Refined("test", "different");

            Assert.Equal(
                "something",
                props.Value("test")
            );
        }
    }
}
