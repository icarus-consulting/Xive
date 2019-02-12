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
    public sealed class SyncXocument : IXocument
    {
        private readonly string name;
        private readonly IXocument origin;
        private readonly IScalar<Mutex> mtx;

        /// <summary>
        /// A Xocument which is accessed exclusively.
        /// </summary>
        public SyncXocument(string name, IXocument origin) : base()
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
