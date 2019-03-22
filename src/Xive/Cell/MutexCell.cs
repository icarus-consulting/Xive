﻿//MIT License

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
using Yaapii.Atoms.Text;

namespace Xive.Cell
{
    /// <summary>
    /// A cell that is system-wide exclusive for one access at a time.
    /// </summary>
    public sealed class MutexCell : ICell
    {
        private readonly IList<Mutex> mtx;
        private readonly ICell cell;

        /// <summary>
        /// A cell that is system-wide exclusive for one access at a time.
        /// </summary>
        public MutexCell(ICell origin)
        {
            lock (origin)
            {
                this.cell = origin;
                this.mtx = new List<Mutex>();
            }
        }

        public string Name()
        {
            Block();
            return this.cell.Name();
        }

        public byte[] Content()
        {
            Block();
            byte[] result = new byte[0];
            result = this.cell.Content();
            return result;
        }

        public void Update(IInput content)
        {
            try
            {
                Block();
                this.cell.Update(content);
            }
            catch (AbandonedMutexException ex)
            {
                throw new ApplicationException($"Cannot get exclusive access to {this.cell.Name()}: {ex.Message}", ex);
            }
            catch (ObjectDisposedException ox)
            {
                throw new ApplicationException($"Cannot get exclusive access to {this.cell.Name()}: {ox.Message}", ox);
            }
            catch (InvalidOperationException ix)
            {
                throw new ApplicationException($"Cannot get exclusive access to {this.cell.Name()}: {ix.Message}", ix);
            }
        }

        public void Dispose()
        {
            lock (this.mtx)
            {
                if (this.mtx.Count == 1)
                {
                    try
                    {
                        this.mtx[0].ReleaseMutex();
                        this.mtx[0].Dispose();
                        this.mtx.Clear();
                    }
                    catch (ObjectDisposedException)
                    {
                        //Do nothing.
                    }
                    catch (ApplicationException ex)
                    {
                        throw new ApplicationException($"Cannot release mutex for cell '{this.cell.Name()}'. Did you try to dispose the cell in another thread in which you called .Content() or Update()?", ex);
                    }
                }
                else if (this.mtx.Count > 1)
                {
                    throw new ApplicationException("Internal error: Duplicate mutex found for " + this.cell.Name());
                }
            }
        }

        private void Block()
        {
            lock (this.mtx)
            {
                var name = this.cell.Name();
                if (this.mtx.Count == 0)
                {
                    var hash =
                        $"Global/" +
                            new TextOf(
                                new BytesBase64(
                                    new Md5DigestOf(
                                        new InputOf(
                                            new BytesOf(
                                                new InputOf(name)
                                            )
                                        )
                                    )
                                ).AsBytes()
                            ).AsString().Replace("/", "_").Replace("\\", "_");

                    this.mtx.Add(new Mutex(false, hash));
                    this.mtx[0].WaitOne();
                }
                if (this.mtx.Count > 1)
                {
                    throw new ApplicationException("Internal error: Duplicate mutex found for " + name);
                }
            }
        }

        ~MutexCell()
        {
            lock (this.mtx)
            {
                if (this.mtx.Count > 0)
                {
                    throw new AbandonedMutexException($"A mutex has not been released for cell named '{this.cell.Name()}'. Did you forget to put it into a using block before calling Content() or Update()?");
                }
                Dispose();
            }
        }
    }
}