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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Data stored in files.
    /// </summary>
    public sealed class DataInFiles : IMemory<byte[]>
    {
        private readonly string root;
        private readonly ISyncPipe sync;
        private readonly bool writeAsync;

        /// <summary>
        /// Data stored in files.
        /// The access to the files is exclusive for this object, but not exclusive system-wide.
        /// </summary>
        public DataInFiles(string root) : this(root, new LocalSyncPipe())
        { }

        /// <summary>
        /// Data stored in files.
        /// The access to files made exclusive by the given pipe.
        /// </summary>
        /// <param name="writeAsync">if true, updating is done asynchronously.</param>
        public DataInFiles(string root, ISyncPipe sync, bool writeAsync = false)
        {
            this.root = root;
            this.sync = sync;
            this.writeAsync = writeAsync;
        }

        public byte[] Content(string name, Func<byte[]> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var result = new byte[0];
            this.sync.Flush(name, () =>
            {
                if (!File.Exists(Path(name)))
                {
                    result = ifAbsent();
                    Update(name, result);
                }
                else
                {
                    result = File.ReadAllBytes(Path(name));
                }
            });
            return result;
        }

        public IEnumerable<string> Knowledge()
        {
            IEnumerable<string> knowledge = new List<string>();
            if (Directory.Exists(this.root))
            {
                knowledge =
                    new Mapped<string, string>(
                        path => path.Substring($"{this.root}/".Length).Replace('\\', '/'),
                        Directory.GetFiles(this.root, "*", SearchOption.AllDirectories)
                    );
            }
            return knowledge;
        }

        public bool Knows(string name)
        {
            return File.Exists(Path(name));
        }

        public void Update(string name, byte[] content)
        {
            if (this.writeAsync)
            {
                Task.Run(() =>
                {
                    Save(name, content);
                });
            }
            else
            {
                Save(name, content);
            }
        }

        private void Save(string name, byte[] content)
        {
            var path = Path(name);
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (content.Length > 0)
            {
                this.sync.Flush(name, () =>
                {
                    using (FileStream f = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        f.Seek(0, SeekOrigin.Begin);
                        f.SetLength(content.Length);
                        f.Write(content, 0, content.Length);
                    }
                });
            }
            else
            {
                this.sync.Flush(name, () =>
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                });
            }
        }

        private string Path(string name)
        {
            return new Normalized(System.IO.Path.Combine(root, name)).AsString();
        }
    }
}
