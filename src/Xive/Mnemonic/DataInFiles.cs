using System;
using System.Collections.Generic;
using System.IO;
using Xive.Hive;
using Yaapii.Atoms.Enumerable;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Data stored in files.
    /// </summary>
    public sealed class DataInFiles : IMemory<MemoryStream>
    {
        private readonly string root;

        /// <summary>
        /// Data stored in files.
        /// </summary>
        public DataInFiles(string root)
        {
            this.root = root;
        }

        public MemoryStream Content(string name, Func<MemoryStream> ifAbsent)
        {
            var result = new MemoryStream();
            if (!File.Exists(Path(name)))
            {
                Update(name, ifAbsent());
            }
            if (File.Exists(Path(name)))
            {
                result = new MemoryStream(File.ReadAllBytes(Path(name)));
            }
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
        }

        private string Path(string name)
        {
            return new Normalized(System.IO.Path.Combine(root, name)).AsString();
        }
    }
}
