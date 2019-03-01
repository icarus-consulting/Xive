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
using System.IO;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which exists physically as a file.
    /// </summary>
    public sealed class FileCell : ICell
    {
        private readonly IScalar<string> path;

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        public FileCell(string root, string name) : this(
            new SolidScalar<string>(() =>
                Path.Combine(root, new StrictCellName(name).AsString())
            )
        )
        { }

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        public FileCell(string path) : this(new ScalarOf<string>(path))
        { }

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        private FileCell(IScalar<string> path)
        {
            this.path = 
                new SolidScalar<string>(() =>
                {
                    var pth = path.Value();
                    if (!Path.IsPathRooted(pth))
                    {
                        throw new ArgumentException($"Cannot work with path '{pth}' because it is not rooted.");
                    }
                    var full = Path.GetFullPath(pth);
                    Validate(full);

                    return full;
                });
        }

        public string Name()
        {
            return this.path.Value();
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];

            if (File.Exists(this.path.Value()))
            {
                result =
                    new BytesOf(
                        new InputOf(new Uri(this.path.Value()))
                    ).AsBytes();
            }
            return result;
        }

        public void Update(IInput content)
        {
            if (content.Stream().Length == 0)
            {
                Cleanup();
            }
            else
            {
                Write(content);
            }
        }

        private void Validate(string path)
        {
            if ((Path.GetFileName(path) + String.Empty).Length == 0)
            {
                throw new ArgumentException($"Cannot work with path '{path}' because it is not a file.");
            }

            var dir = new StrictCoordinate(Path.GetDirectoryName(path)).AsString();
            var file = new StrictCellName(Path.GetFileName(path)).AsString();
        }

        private void Write(IInput content)
        {
            lock (this)
            {
                var dir = Path.GetDirectoryName(this.path.Value());
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllBytes(
                    this.path.Value(),
                    new BytesOf(content).AsBytes()
                );
            }
        }

        private void Cleanup()
        {
            lock (this)
            {
                if (File.Exists(this.path.Value()))
                {
                    File.Delete(this.path.Value());
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
