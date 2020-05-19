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

using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memories which a stored in a cache.
    /// Update is transferred to the origin memories.
    /// </summary>
    public sealed class CachedMemories : IMnemonic
    {
        private readonly IMnemonic origin;
        private readonly IScalar<IMemory<byte[]>> data;
        private readonly IScalar<IMemory<XNode>> xml;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="maxSize"></param>
        public CachedMemories(IMnemonic origin, params string[] items) : this(origin, int.MaxValue, new List<string>(new ManyOf<string>(items)))
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="maxSize"></param>
        public CachedMemories(IMnemonic origin) : this(origin, int.MaxValue, new List<string>())
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="maxSize"></param>
        public CachedMemories(IMnemonic origin, int maxSize) : this(origin, maxSize, new List<string>())
        { }

        /// <summary>
        /// Memories which a stored in a cache.
        /// Update is transferred to the origin memories.
        /// </summary>
        public CachedMemories(IMnemonic origin, int maxSize, IEnumerable<string> blacklist) : this(
            origin,
            maxSize,
            new SolidList<string>(
                new Yaapii.Atoms.List.Mapped<string, string>(
                    entry => new Normalized(entry).AsString(),
                    blacklist
                )
            )
        )
        { }

        /// <summary>
        /// Memories which a stored in a cache.
        /// Update is transferred to the origin memories.
        /// </summary>
        internal CachedMemories(IMnemonic origin, int maxSize, IList<string> blacklist)
        {
            this.origin = origin;
            this.data =
                new Solid<IMemory<byte[]>>(() =>
                    new CachedMemory<byte[]>(
                        origin.Data(),
                        maxSize,
                        bytes => bytes.Length == 0,
                        blacklist
                    )
                );
            this.xml =
                new Solid<IMemory<XNode>>(() =>
                    new CachedMemory<XNode>(
                        origin.XML(),
                        maxSize,
                        node => CheckEmpty(node),
                        blacklist
                    )
                );
        }

        private static bool CheckEmpty(XNode node)
        {
            var result = false;
            if (node.Document == null)
            {
                result = true;
            }
            else if (node.Document.Root == null)
            {
                result = true;
            }
            else if (node.Document.Root.IsEmpty)
            {
                result = true;
            }

            return result;
        }

        public IMemory<byte[]> Data()
        {
            return this.data.Value();
        }

        public IProps Props(string scope, string id)
        {
            return this.origin.Props(scope, id); //props are already cached by design.
        }

        public IMemory<XNode> XML()
        {
            return this.xml.Value();
        }
    }
}
