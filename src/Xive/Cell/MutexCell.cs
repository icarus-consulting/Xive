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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Yaapii.Atoms;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Cell
{
    /// <summary>
    /// A cell that is system-wide exclusive for one access at a time.
    /// </summary>
    public sealed class MutexCell : ICell
    {
        private readonly IList<Mutex> mtx;
        private readonly string name;
        private readonly ICell cell;

        /// <summary>
        /// A cell that is system-wide exclusive for one access at a time.
        /// </summary>
        public MutexCell(string name, ICell origin)
        {
            this.cell = origin;
            this.mtx = new List<Mutex>();
            this.name = name;
        }

        public byte[] Content()
        {
            lock (this.mtx)
            {
                byte[] result = new byte[0];
                Block();
                result = this.cell.Content();
                return result;
            }
        }

        public void Update(IInput content)
        {
            lock (this.mtx)
            {
                try
                {
                    Block();
                    Debug.WriteLine("STARTED " + this.name);
                    this.cell.Update(content);
                }
                catch (AbandonedMutexException ex)
                {
                    throw new ApplicationException($"Cannot get exclusive access to {this.name}: {ex.Message}", ex);
                }
                catch (ObjectDisposedException ox)
                {
                    throw new ApplicationException($"Cannot get exclusive access to {this.name}: {ox.Message}", ox);
                }
                catch (InvalidOperationException ix)
                {
                    throw new ApplicationException($"Cannot get exclusive access to {this.name}: {ix.Message}", ix);
                }
            }

        }

        public void Dispose()
        {
            try
            {
                lock (this.mtx)
                {
                    if (this.mtx.Count == 1)
                    {
                        this.mtx[0].ReleaseMutex();
                        Debug.WriteLine("Released " + this.name);
                    }
                    else if (this.mtx.Count > 1)
                    {
                        throw new ApplicationException("Duplicate mutex found for " + name);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                //Do nothing.
            }
            catch (ApplicationException ex)
            {
                //Do nothing.
            }
        }

        private void Block()
        {
            lock (this.mtx)
            {
                if (this.mtx.Count == 0)
                {
                    this.mtx.Add(
                        new Mutex(
                            false,
                            $"Global/" +
                            new TextOf(
                                new BytesBase64(
                                    new Md5DigestOf(
                                        new InputOf(
                                            new BytesOf(
                                                new InputOf(this.name)
                                            )
                                        )
                                    )
                                ).AsBytes()
                            ).AsString().Replace("/", "_").Replace("\\", "_")
                        )
                    );
                    this.mtx[0].WaitOne();
                }
                if (this.mtx.Count > 1)
                {
                    throw new ApplicationException("Duplicate mutex found for " + name);
                }
            }
        }

        ~MutexCell()
        {
            Dispose();
        }
    }
}
