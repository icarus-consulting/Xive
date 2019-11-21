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
using System.Xml.Linq;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument
{
    /// <summary>
    /// A Xocument which is accessed exclusively.
    /// </summary>
    public sealed class MutexXocument : IXocument
    {
        private readonly string name;
        private readonly IXocument origin;
        private readonly IList<Mutex> mtx;

        /// <summary>
        /// A Xocument which is accessed exclusively.
        /// </summary>
        public MutexXocument(string name, IXocument origin) : base()
        {
            lock (this)
            {
                this.name = name;
                this.origin = origin;
                this.mtx = new List<Mutex>();
            }
        }

        public void Modify(IEnumerable<IDirective> dirs)
        {
            Block();
            this.origin.Modify(dirs);
        }

        public XNode Node()
        {
            Block();
            return this.origin.Node();
        }

        public IList<IXML> Nodes(string xpath)
        {
            IList<IXML> result;
            Block();
            result = this.origin.Nodes(xpath);
            return result;
        }

        public string Value(string xpath, string def)
        {
            string result = String.Empty;
            Block();
            result = this.origin.Value(xpath, def);
            return result;
        }

        public IList<string> Values(string xpath)
        {
            IList<string> result;
            Block();
            result = this.origin.Values(xpath);
            return result;
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
                        throw new ApplicationException($"Cannot release mutex for xocument '{this.name}'. "
                            + "Did you try to dispose the xocument in another thread than "
                            + "the one where you called its read/modify methods?",
                            ex
                        );
                    }
                }
                else if (this.mtx.Count > 1)
                {
                    throw new ApplicationException("Internal error: Duplicate mutex found for " + name);
                }
            }

            this.origin.Dispose();
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
                            $"Local/" +
                            new TextOf(
                                new BytesBase64(
                                    new Md5DigestOf(
                                        new InputOf(
                                            new BytesOf(
                                                new InputOf(
                                                    this.name.Replace("/", "_").Replace("\\", "_")
                                                )
                                            )
                                        )
                                    )
                                ).AsBytes()
                            ).AsString()
                        )
                    );
                    this.mtx[0].WaitOne();
                }
                if (this.mtx.Count > 1)
                {
                    throw new ApplicationException($"Internal error: Duplicate mutex found for '{name}'");
                }
            }
        }

        ~MutexXocument()
        {
            lock (this.mtx)
            {
                if (this.mtx.Count > 0)
                {
                    throw new AbandonedMutexException($"A mutex has not been released for xocument '{this.name}'. Did you forget to put it into a using block before calling its methods?");
                }
                Dispose();
            }
        }
    }
}
