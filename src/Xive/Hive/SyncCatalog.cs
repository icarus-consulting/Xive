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
using System.Diagnostics;
using System.Threading;
using Yaapii.Xambly;

namespace Xive.Hive
{
    /// <summary>
    /// A catalog to manage a list of combs in a hive processwide exclusively.
    /// </summary>
    public sealed class SyncCatalog : ICatalog
    {
        private readonly IHive hive;
        private readonly ICatalog catalog;
        private readonly ISyncValve valve;
        private readonly int[] locked;

        /// <summary>
        /// A catalog to manage a list of combs in a hive processwide exclusively.
        /// </summary>
        public SyncCatalog(IHive hive, ICatalog origin, ISyncValve syncValve)
        {
            this.hive = hive;
            this.catalog = origin;
            this.valve = syncValve;
            this.locked = new int[1] { 0 };
        }

        public void Update(string id, IEnumerable<IDirective> content)
        {
            try
            {
                Block();
                this.catalog.Update(id, content);
            }
            finally
            {
                Dispose();
            }
        }

        public IEnumerable<string> List(string xpath)
        {
            try
            {
                Block();
                return this.catalog.List(xpath);
            }
            finally
            {
                Dispose();
            }
        }

        public void Remove(string id)
        {
            try
            {
                Block();
                this.catalog.Remove(id);
            }
            finally
            {
                Dispose();
            }
        }

        public void Create(string id)
        {
            try
            {
                Block();
                this.catalog.Create(id);
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            lock (this.catalog)
            {
                var name = this.hive.HQ().Name();
                var locks = this.locked[0];
                this.locked[0] = 0;
                for (int i = 0; i < locks; i++)
                {
                    this.valve.Mutex($"{name}/catalog.xml").ReleaseMutex();
                }
                Debug.WriteLine("Unblocked from " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void Block()
        {
            var name = this.hive.HQ().Name();
            this.valve.Mutex($"{name}/catalog.xml").WaitOne();
            Debug.WriteLine("Blocked from " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            this.locked[0]++;
        }

        ~SyncCatalog()
        {

            if (this.locked[0] > 0)
            {
                //Dispose();
                throw new AbandonedMutexException($"A mutex has not been released for catalog '{this.hive.HQ().Name()}'. Did you forget to put it into a using block before calling its methods?");
            }

        }
    }
}
