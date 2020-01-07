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

using Xive.Cell;
using Xive.Xocument;

namespace Xive.Comb
{
    /// <summary>
    /// A comb that accesses cells systemwide exclusively.
    /// </summary>
    public sealed class SyncComb : IHoneyComb
    {
        private readonly IHoneyComb comb;
        private readonly ISyncValve syncValve;

        /// <summary>
        /// A comb that accesses cells systemwide exclusively.
        /// </summary>
        public SyncComb(IHoneyComb comb) : this(comb, new ProcessSyncValve())
        { }

        /// <summary>
        /// A comb that accesses cells systemwide exclusively.
        /// </summary>
        public SyncComb(IHoneyComb comb, ISyncValve syncValve)
        {
            this.comb = comb;
            this.syncValve = syncValve;
        }

        public string Name()
        {
            lock (comb)
            {
                return comb.Name();
            }
        }

        public ICell Cell(string name)
        {
            return new SyncCell(comb.Cell(name), this.syncValve);
        }

        public IXocument Xocument(string name)
        {
            return 
                new SyncXocument(
                    $"{comb.Name()}/{name}",
                    this.comb.Xocument(name),
                    this.syncValve
                );
        }
    }
}
