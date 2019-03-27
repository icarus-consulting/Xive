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
using System.IO;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell whose content is cached in memory.
    /// </summary>
    public sealed class CachedCell : ICell
    {
        private readonly ICell origin;
        private readonly string name;
        private readonly IScalar<IDictionary<string, MemoryStream>> memory;
        private readonly int maxSize;

        /// <summary>
        /// A cell whose content is cached in memory.
        /// </summary>
        public CachedCell(ICell origin, string name, IDictionary<string, MemoryStream> binMemory, IDictionary<string, XNode> xmlMemory, int maxBytes = 10485760)
        {
            this.origin = origin;
            this.name = name;
            this.memory =
                new ScalarOf<IDictionary<string, MemoryStream>>(() =>
                {
                    if(xmlMemory.ContainsKey(name))
                    {
                        throw 
                            new InvalidOperationException(
                                $"Cannot use '{name}' as binary cell because it has been stored as xml previously. "
                                + "Caching works only if you use data consistently."
                            );
                    }
                    return binMemory;
                });
            this.maxSize = maxBytes;
        }

        public string Name()
        {
            return this.name;
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];
            if (!this.memory.Value().ContainsKey(this.name))
            {
                result = this.origin.Content();
                if (result.Length <= this.maxSize)
                {
                    this.memory.Value()[this.name] = new MemoryStream(result);
                }
            }
            else
            {
                this.memory.Value()[this.name].Seek(0, SeekOrigin.Begin);
                result = this.memory.Value()[this.name].ToArray();
            }
            return result;
        }

        public void Update(IInput content)
        {
            var stream = content.Stream();
            if (stream.Length <= this.maxSize)
            {
                var copy = new MemoryStream();
                stream.CopyTo(copy);
                stream.Seek(0, SeekOrigin.Begin);
                copy.Seek(0, SeekOrigin.Begin);
                this.memory.Value()[this.name] = copy;
            }
            else
            {
                this.memory.Value().Remove(this.name);
            }
            this.origin.Update(content);
        }

        public void Dispose()
        {
            this.origin.Dispose();
        }
    }
}