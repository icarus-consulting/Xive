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
    public sealed class SyncCell : ICell
    {
        private readonly IScalar<Mutex> mtx;
        private readonly string name;
        private readonly IScalar<ICell> cell;

        /// <summary>
        /// A cell that is system-wide exclusive for one access at a time.
        /// </summary>
        public SyncCell(string name, Func<string, ICell> origin)
        {
            lock (this) //make creation of mutex solid. Otherwise odd behaviour occures because creation can be left unfinished and mutex abandoned.
            {
                this.cell = new StickyScalar<ICell>(() => origin(this.name));
                this.name = name;
                this.mtx =
                    new SolidScalar<Mutex>(() =>
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
            }
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];
            try
            {
                this.mtx.Value().WaitOne();
                result = cell.Value().Content();
            }
            finally
            {
                Dispose();
            }
            return result;
        }

        public void Update(IInput content)
        {
            try
            {
                this.mtx.Value().WaitOne();
                this.cell.Value().Update(content);
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
            finally
            {
                Dispose();
            }

        }

        public void Dispose()
        {
            try
            {
                this.mtx.Value().ReleaseMutex();
            }
            catch (ObjectDisposedException)
            {
                //Do nothing.
            }
            catch (ApplicationException)
            {
                //Do nothing.
            }
        }

        ~SyncCell()
        {
            Dispose();
        }
    }
}
