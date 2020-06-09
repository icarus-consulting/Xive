//MIT License

//Copyright (c) 2020 ICARUS Consulting GmbH

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

using System.Threading;
using Yaapii.Atoms;

namespace Xive.Cell
{
    /// <summary>
    /// A cell that is synced in context of the owning process.
    /// </summary>
    public sealed class SyncCell : ICell
    {
        private readonly ISyncValve valve;
        private readonly ICell origin;
        private readonly int[] locked;

        public SyncCell(ICell origin) : this(origin, new SyncGate())
        { }

        public SyncCell(ICell origin, ISyncValve valve)
        {
            lock (this)
            {
                this.valve = valve;
                this.origin = origin;
                this.locked = new int[1] { 0 };
            }
        }

        public string Name()
        {
            return this.origin.Name();
        }

        public byte[] Content()
        {
            byte[] result;
            Block();
            result = origin.Content();
            Dispose();
            return result;
        }

        public void Dispose()
        {
            lock (this.origin)
            {
                this.origin.Dispose();
                var locks = this.locked[0];
                this.locked[0] = 0;
                for (int i = 0; i < locks; i++)
                {
                    this.valve.Mutex(this.origin.Name()).ReleaseMutex();
                }
            }

        }

        public void Update(IInput content)
        {
            Block();
            origin.Update(content);
            Dispose();
        }

        private void Block()
        {
            this.valve.Mutex(this.origin.Name()).WaitOne();
            this.locked[0]++;
        }

        ~SyncCell()
        {

            if (this.locked[0] > 0)
            {
                Dispose();
                throw new AbandonedMutexException($"A mutex has not been released for cell '{this.origin.Name()}'.");
            }

        }
    }
}
