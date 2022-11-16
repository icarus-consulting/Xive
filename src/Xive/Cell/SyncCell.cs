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

using Xive.Mnemonic;
using Yaapii.Atoms;

namespace Xive.Cell
{
    /// <summary>
    /// A cell that is synced in context of the owning process.
    /// </summary>
    public sealed class SyncCell : ICell
    {
        private readonly ISyncPipe sync;
        private readonly ICell origin;
        private readonly int[] locked;

        public SyncCell(ICell origin, ISyncPipe sync)
        {
            this.sync = sync;
            this.origin = origin;
        }

        public string Name()
        {
            return this.origin.Name();
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];
            this.sync.Flush(origin.Name(), () =>
            {
                result = origin.Content();
            });
            return result;
        }

        public void Dispose()
        {

        }

        public void Update(IInput content)
        {
            this.sync.Flush(origin.Name(), () =>
            {
                origin.Update(content);
            });
        }
    }
}
