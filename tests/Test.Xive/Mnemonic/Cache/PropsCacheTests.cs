//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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

using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;

namespace Xive.Mnemonic.Cache.Test
{
    public sealed class PropsCacheTests
    {
        [Fact]
        public void CachesOnRead()
        {
            var reads = 0;
            var cache = new PropsCache();

            cache.Content("test", () => { reads++; return new ConcurrentDictionary<string, string[]>(); });
            cache.Content("test", () => { reads++; return new ConcurrentDictionary<string, string[]>(); });

            Assert.Equal(1, reads);
        }

        [Fact]
        public void CachesOnUpdate()
        {
            var reads = 0;
            var cache = new PropsCache();

            cache.Content("test", () => new ConcurrentDictionary<string, string[]>());
            cache.Update("test",
                () => { reads++; return new ConcurrentDictionary<string, string[]>(); },
                () => { reads++; return new ConcurrentDictionary<string, string[]>(); }
            );

            cache.Content("test", () => { reads++; return new ConcurrentDictionary<string, string[]>(); });

            Assert.Equal(1, reads);
        }

        [Fact]
        public void Removes()
        {
            var reads = 0;
            var cache = new PropsCache();

            cache.Content("test", () => { reads++; return new ConcurrentDictionary<string, string[]>(); });
            cache.Remove("test");
            cache.Content("test", () => { reads++; return new ConcurrentDictionary<string, string[]>(); });

            Assert.Equal(2, reads);
        }

        [Fact]
        public void IsThreadSafe()
        {
            var reads = 0;
            var cache = new PropsCache();

            Parallel.For(0, 64, (idx) =>
            {
                cache.Content("test", () => new ConcurrentDictionary<string, string[]>());
                cache.Update("test",
                    () => { reads++; return new ConcurrentDictionary<string, string[]>(); },
                    () => { reads++; return new ConcurrentDictionary<string, string[]>(); }
                );

                cache.Content("test", () => { reads++; return new ConcurrentDictionary<string, string[]>(); });
            });

            Assert.Equal(64, reads);
        }
    }
}
