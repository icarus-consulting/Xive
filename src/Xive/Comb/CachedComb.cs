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

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Xive.Cell;
using Xive.Hive;
using Xive.Xocument;

namespace Xive.Comb
{
    /// <summary>
    /// A comb that is cached in memory.
    /// </summary>
    public sealed class CachedComb : IHoneyComb
    {
        private readonly ICache cache;
        private readonly IHoneyComb comb;

        /// <summary>
        /// A comb that is cached in memory.
        /// </summary>
        public CachedComb(IHoneyComb comb, ICache cache)
        {
            this.comb = comb;
            this.cache = cache;
        }

        public ICell Cell(string name)
        {
            return
                new CachedCell(
                    this.comb.Cell(name),
                    $"{this.comb.Name()}/{name}",
                    this.cache
                );
        }

        public string Name()
        {
            return this.comb.Name();
        }

        public IProps Props()
        {
            throw new System.NotImplementedException();
        }

        public IXocument Xocument(string name)
        {
            return
                new CachedXocument(
                    $"{this.comb.Name()}/{name}",
                    this.comb.Xocument(name),
                    this.cache
                );
        }
    }
}
