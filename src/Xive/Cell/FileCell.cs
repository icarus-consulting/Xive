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

using System;
using System.IO;
using Xive.Mnemonic;
using Yaapii.Atoms;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which exists physically as a file.
    /// </summary>
    public sealed class FileCell : ICell
    {
        private readonly IScalar<IMnemonic> mem;
        private readonly IScalar<string> name;

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        public FileCell(string path) : this(
            new ScalarOf<string>(() =>
            {
                if (!Path.IsPathRooted(path))
                {
                    throw new ArgumentException($"Cannot use '{path}' as FileCell path because the path must be rooted.");
                }
                var name = Path.GetFileName(path);
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentException($"Cannot use '{path}' as FileCell path because the path must be rooted and have a file name.");
                }
                return name;
            }),
            new ScalarOf<IMnemonic>(() =>
            {
                if (!Path.IsPathRooted(path))
                {
                    throw new ArgumentException($"Cannot use '{path}' as FileCell path because the path must be rooted.");
                }
                var root = Path.GetDirectoryName(path);
                return new FileMnemonic(root);
            })
        )
        { }

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        private FileCell(IScalar<string> name, ScalarOf<IMnemonic> mem)
        {
            this.mem = mem;
            this.name = name;
        }

        public string Name()
        {
            return this.name.Value();
        }

        public byte[] Content()
        {
            return
                this.mem
                    .Value()
                    .Contents()
                    .Bytes(this.name.Value(), () => new byte[0]);
        }

        public void Update(IInput content)
        {
            var stream = content.Stream();
            if (stream.Length > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
                this.mem.Value().Contents().UpdateBytes(this.name.Value(), new BytesOf(content).AsBytes());
            }
            else
            {
                this.mem.Value().Contents().UpdateBytes(this.name.Value(), new byte[0]);
            }
        }

        public void Dispose()
        { }
    }
}
