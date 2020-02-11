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
    public sealed class DataInFiles : IMemory<MemoryStream>
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

        public MemoryStream Content(string name, Func<MemoryStream> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var result = new MemoryStream();
            this.sync.Flush(name, () =>
            {
                if (!File.Exists(Path(name)))
                {
                    result = ifAbsent();
                    Update(name, result);
                }
                else
                {
                    result = new MemoryStream(File.ReadAllBytes(Path(name)));
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
                        path => path.Substring($"{this.root}/".Length),
                        Directory.GetFiles(this.root, "*", SearchOption.AllDirectories)
                    );
            }
            return knowledge;
        }

        public bool Knows(string name)
        {
            return File.Exists(Path(name));
        }

        public void Update(string name, MemoryStream content)
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

        private void Save(string name, MemoryStream content)
        {
            name = new Normalized(name).AsString();
            var result = new MemoryStream();
            this.sync.Flush(name, () =>
            {
                if (content.Length > 0)
                {
                    var path = Path(name);
                    var dir = System.IO.Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    using (FileStream f = File.Open(Path(name), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        f.Seek(0, SeekOrigin.Begin);
                        f.SetLength(content.Length);
                        content.Seek(0, SeekOrigin.Begin);
                        content.CopyTo(f);
                    }
                }
                else
                {
                    if (File.Exists(Path(name)))
                    {
                        File.Delete(Path(name));
                    }
                }
            });
        }

        private string Path(string name)
        {
            return new Normalized(System.IO.Path.Combine(root, name)).AsString();
        }
    }
}
