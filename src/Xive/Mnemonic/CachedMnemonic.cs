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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xive.Mnemonic.Cache;
using Xive.Mnemonic.Content;
using Xive.Props;
using Yaapii.Atoms;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic
{
    /// <summary>
    /// A cached mnemonic which caches bytes and parsed XML.
    /// </summary>
    public sealed class CachedMnemonic : IMnemonic
    {
        private readonly IScalar<IContents> contents;
        private readonly ICache<ConcurrentDictionary<string, string[]>> props;
        private readonly IMnemonic origin;

        /// <summary>
        /// A cached mnemonic which caches bytes and parsed XML.
        /// </summary>
        public CachedMnemonic(IMnemonic origin, long maxSize) : this(origin, new ManyOf<string>(), maxSize, new PropsCache())
        { }

        /// <summary>
        /// A cached mnemonic which caches bytes and parsed XML.
        /// </summary>
        public CachedMnemonic(IMnemonic origin, params string[] ignored) : this(origin, new ManyOf<string>(ignored), Int64.MaxValue, new PropsCache())
        { }

        /// <summary>
        /// A cached mnemonic which caches bytes and parsed XML.
        /// </summary>
        public CachedMnemonic(IMnemonic origin) : this(origin, new ManyOf<string>(), Int64.MaxValue, new PropsCache())
        { }

        public CachedMnemonic(IMnemonic origin, IEnumerable<string> ignored, long maxSize, ICache<ConcurrentDictionary<string, string[]>> propsCache) : this(
            origin, new ScalarOf<IContents>(() => new CachedContents(origin.Contents(), ignored, maxSize)), propsCache)
        { }

        public CachedMnemonic(IMnemonic origin, ICache<byte[]> byteCache, ICache<XNode> xmlCache, ICache<ConcurrentDictionary<string, string[]>> propsCache) : this(
            origin, new ScalarOf<IContents>(() => new CachedContents(origin.Contents(), byteCache, xmlCache)), propsCache)
        { }

        /// <summary>
        /// A cached mnemonic which caches bytes and parsed XML.
        /// </summary>
        private CachedMnemonic(IMnemonic origin, IScalar<IContents> contents, ICache<ConcurrentDictionary<string, string[]>> propsCache)
        {
            this.props = propsCache;
            this.contents = contents;
            this.origin = origin;
        }

        public IContents Contents()
        {
            return this.contents.Value();
        }

        public IProps Props(string scope, string id)
        {
            var key = $"{scope}::{id}";
            lock (this.props)
            {
                var originProps = this.origin.Props(scope, id);
                return
                    new CachedProps(
                        originProps,
                        this.props.Content(key, () =>
                        {
                            var result = new ConcurrentDictionary<string, string[]>();
                            foreach (var name in originProps.Names())
                            {
                                result.TryAdd(name, originProps.Values(name).ToArray());
                            }
                            return result;
                        })
                    );
            }
        }
    }
}
