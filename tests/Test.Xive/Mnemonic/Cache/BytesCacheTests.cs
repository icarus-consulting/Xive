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

using System.Threading.Tasks;
using Xive.Mnemonic.Cache;
using Xunit;

namespace Xive.Test.Mnemonic.Cache
{
    public sealed class BytesCacheTests
    {
        [Fact]
        public void CachesOnRead()
        {
            var reads = 0;
            var cache = new BytesCache();

            cache.Content("test", () => { reads++; return new byte[1] { 0xAB }; });
            cache.Content("test", () => { reads++; return new byte[1] { 0xAB }; });

            Assert.Equal(1, reads);
        }

        [Fact]
        public void CachesOnUpdate()
        {
            var reads = 0;
            var cache = new BytesCache();

            cache.Content("test", () => new byte[1] { 0xAA });
            cache.Update("test",
                () => { reads++; return new byte[1] { 0xBB }; },
                () => { reads++; return new byte[1] { 0xCC }; }
            );

            cache.Content("test", () => { reads++; return new byte[1] { 0xDD }; });

            Assert.Equal(1, reads);
        }

        [Fact]
        public void Removes()
        {
            var reads = 0;
            var cache = new BytesCache();

            cache.Content("test", () => { reads++; return new byte[1] { 0xDD }; });
            cache.Remove("test");
            cache.Content("test", () => { reads++; return new byte[1] { 0xDD }; });

            Assert.Equal(2, reads);
        }

        [Fact]
        public void IsThreadSafe()
        {
            var reads = 0;
            var cache = new BytesCache();

            Parallel.For(0, 64, (idx) =>
            {
                cache.Content("test", () => new byte[1] { 0xAA });
                cache.Update("test",
                    () => { reads++; return new byte[1] { 0xBB }; },
                    () => { reads++; return new byte[1] { 0xCC }; }
                );

                cache.Content("test", () => { reads++; return new byte[1] { 0xDD }; });
            });

            Assert.Equal(64, reads);
        }


        [Fact]
        public void RemovesOldValueOnExceedingMax()
        {
            var cache = new BytesCache(maxSize: 110);

            cache.Update("test",
                () => new byte[80],
                () => new byte[80]
            );
            cache.Update("test",
                () => new byte[220],
                () => new byte[220]
            );

            Assert.Equal(
                10,
                cache.Content("test", ()=>new byte[10]).Length
            );
        }
    }
}
