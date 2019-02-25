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
using System.Threading;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
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
        private readonly IScalar<Mutex> mtx;

        /// <summary>
        /// A Xocument which is accessed exclusively.
        /// </summary>
        public MutexXocument(string name, IXocument origin) : base()
        {
            this.name = name;
            this.origin = origin;
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

        public void Modify(IEnumerable<IDirective> dirs)
        {
            try
            {
                this.mtx.Value().WaitOne();
                this.origin.Modify(dirs);
            }
            finally
            {
                Dispose();
            }
        }

        public XNode Node()
        {
            return this.origin.Node();
        }

        public IList<IXML> Nodes(string xpath)
        {
            IList<IXML> result;
            try
            {
                this.mtx.Value().WaitOne();
                result = this.origin.Nodes(xpath);
            }
            finally
            {
                Dispose();
            }
            return result;
        }

        public string Value(string xpath, string def)
        {
            string result = String.Empty;
            try
            {
                this.mtx.Value().WaitOne();
                result = this.origin.Value(xpath, def);
            }
            finally
            {
                Dispose();
            }
            return result;
        }

        public IList<string> Values(string xpath)
        {
            IList<string> result;
            try
            {
                this.mtx.Value().WaitOne();
                result = this.origin.Values(xpath);
            }
            finally
            {
                Dispose();
            }
            return result;
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
            this.origin.Dispose();
        }
    }
}
