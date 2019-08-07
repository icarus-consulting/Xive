//MIT License

//Copyright (c) 2019 ICARUS Consulting GmbH

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

using System.IO;
using Xive.Hive;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;

namespace Xive.Cell
{
    /// <summary>
    /// A cell whose content is cached in memory.
    /// </summary>
    public sealed class CachedCell : ICell
    {
        private readonly ICell origin;
        private readonly string name;
        private readonly ICache cache;

        /// <summary>
        /// A cell whose content is cached in memory.
        /// </summary>
        public CachedCell(ICell origin, string name, ICache cache)
        {
            this.origin = origin;
            this.name = name;
            this.cache = cache;
        }

        public string Name()
        {
            return this.name;
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];
            var stream =
                this.cache.Binary(
                    this.name,
                    () => new MemoryStream(this.origin.Content())
                );
            stream.Seek(0, SeekOrigin.Begin);
            result = stream.ToArray();
            return result;
        }

        public void Update(IInput content)
        {
            var stream = content.Stream();
            var copy = new MemoryStream();
            stream.CopyTo(copy);
            stream.Seek(0, SeekOrigin.Begin);
            copy.Seek(0, SeekOrigin.Begin);
            this.origin.Update(new InputOf(copy));
            copy.Seek(0, SeekOrigin.Begin);
            this.cache.Remove(this.name);
            this.cache.Binary(this.name, () => { return copy; });
        }

        public void Dispose()
        {
            this.origin.Dispose();
        }
    }
}