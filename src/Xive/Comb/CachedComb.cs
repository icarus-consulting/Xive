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
using Xive.Xocument;

namespace Xive.Comb
{
    /// <summary>
    /// A comb that is cached in memory.
    /// </summary>
    public sealed class CachedComb : IHoneyComb
    {
        private readonly IDictionary<string, byte[]> binMemory;
        private readonly IDictionary<string, XNode> xmlMemory;
        private readonly IHoneyComb comb;
        private readonly int maxBytes;

        /// <summary>
        /// A comb that is cached in memory.
        /// </summary>
        public CachedComb(IHoneyComb comb, IDictionary<string, byte[]> binMemory, IDictionary<string, XNode> xmlMemory, int maxBytes = 10485760)
        {
            this.comb = comb;
            this.binMemory = binMemory;
            this.xmlMemory = xmlMemory;
            this.maxBytes = maxBytes;
        }

        public ICell Cell(string name)
        {
            return 
                new CachedCell(
                    this.comb.Cell(name),
                    $"{this.comb.Name()}{Path.DirectorySeparatorChar}{name}",
                    this.binMemory,
                    this.maxBytes
                );
        }

        public string Name()
        {
            return this.Name();
        }

        public IXocument Xocument(string name)
        {
            return
                new CachedXocument(
                    $"{this.comb.Name()}{Path.DirectorySeparatorChar}{name}",
                    this.comb.Xocument(name),
                    this.xmlMemory
                );
        }
    }
}
