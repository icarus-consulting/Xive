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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    /// <summary>
    /// A Xocument which is accessed exclusively.
    /// </summary>
    public sealed class SyncXocument : IXocument
    {
        private readonly string name;
        private readonly IXocument origin;
        private readonly ISyncValve valve;
        private readonly int[] locked;

        /// <summary>
        /// A Xocument which is accessed exclusively.
        /// </summary>
        public SyncXocument(string name, IXocument origin, ISyncValve sync)
        {
            lock (this)
            {
                this.name = name;
                this.origin = origin;
                this.valve = sync;
                this.locked = new int[1] { 0 };
            }
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            Block();
            this.origin.Modify(dirs);
            Dispose();
        }

        public XNode Node()
        {
            Block();
            XNode node = this.origin.Node();
            Dispose();
            return node;
        }

        public IList<IXML> Nodes(string xpath)
        {
            IList<IXML> result;
            Block();
            result = this.origin.Nodes(xpath);
            Dispose();
            return result;
        }

        public string Value(string xpath, string def)
        {
            string result = String.Empty;
            Block();
            result = this.origin.Value(xpath, def);
            Dispose();
            return result;
        }

        public IList<string> Values(string xpath)
        {
            IList<string> result;
            Block();
            result = this.origin.Values(xpath);
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
                    this.valve.Mutex(this.name).ReleaseMutex();
                }
            }
        }

        private void Block()
        {
            this.valve.Mutex(this.name).WaitOne();
            this.locked[0]++;
        }

        ~SyncXocument()
        {

            if (this.locked[0] > 0)
            {
                Dispose();
                throw new AbandonedMutexException($"A mutex has not been released for xocument '{this.name}'.");
            }

        }
    }
}
