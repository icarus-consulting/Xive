﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xive.Mnemonic.Sync;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic.Content
{
    /// <summary>
    /// Content in files.
    /// Please note that every time you read, the file will be read from disk.
    /// NAlso note that when you read XML, the xml will get parsed every time it is read.
    /// If you want to cache the parsed bytes and parsed XML, use <see cref="CachedContent"/>.
    /// </summary>
    public sealed class FileContents : IContents
    {
        private readonly string root;
        private readonly ISyncPipe sync;
        private readonly bool writeAsync;

        /// <summary>
        /// Content in files.
        /// Please note that every time you read, the file will be read from disk.
        /// NAlso note that when you read XML, the xml will get parsed every time it is read.
        /// If you want to cache the parsed bytes and parsed XML, use <see cref="CachedContent"/>.
        /// </summary>
        public FileContents(string root, bool writeAsync = false) : this(root, new LocalSyncPipe(), writeAsync)
        { }

        /// <summary>
        /// Content in files.
        /// Please note that every time you read, the file will be read from disk.
        /// NAlso note that when you read XML, the xml will get parsed every time it is read.
        /// If you want to cache the parsed bytes and parsed XML, use <see cref="CachedContent"/>.
        /// </summary>
        public FileContents(string root, ISyncPipe sync, bool writeAsync = false)
        {
            this.root = root;
            this.sync = sync;
            this.writeAsync = writeAsync;
        }

        public byte[] Bytes(string name, Func<byte[]> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var result = new byte[0];
            this.sync.Flush(name, () =>
            {
                var path = Path(name);
                if (!File.Exists(path))
                {
                    result = ifAbsent();
                    UpdateBytes(name, result);
                }
                else
                {
                    try
                    {
                        result = File.ReadAllBytes(path);
                    }
                    catch (IOException ex)
                    {
                        throw ExceptionWithFileName(ex, path);
                    }
                }
            });
            return result;
        }

        public IList<string> Knowledge(string filter = "")
        {
            var directory = new Normalized(System.IO.Path.Combine(this.root, filter)).AsString();

            IList<string> result = new ListOf<string>();
            if (Directory.Exists(directory))
            {
                result =
                    new Mapped<string, string>(
                        file => new Normalized(file.Substring(this.root.Length + "/".Length)).AsString(),
                        new ListOf<string>(
                            Directory.GetFiles(
                                new Normalized(System.IO.Path.Combine(this.root, filter)).AsString(),
                                "*",
                                SearchOption.AllDirectories
                            )
                        )
                    );
            }
            return result;
        }

        public void UpdateBytes(string name, byte[] data)
        {
            if (this.writeAsync)
            {
                Task.Run(() =>
                {
                    Save(name, data);
                });
            }
            else
            {
                Save(name, data);
            }
        }

        public void UpdateXml(string name, XNode xml)
        {
            this.UpdateBytes(name, new BytesOf(xml.ToString(), Encoding.UTF8).AsBytes());
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            name = new Normalized(name).AsString();
            XNode result;
            if (!File.Exists(System.IO.Path.Combine(this.root, name)))
            {
                result = ifAbsent();
                this.sync.Flush(name, () =>
                {
                    if (result.Document.Elements().GetEnumerator().MoveNext())
                    {
                        UpdateXml(name, result);
                    }
                });
            }
            else
            {
                result = Parsed(name, Bytes(name, () => throw new ApplicationException($"Internal error, assumed to never access ifAbsent() method here.")));
            }
            return result;
        }

        private void Save(string name, byte[] content)
        {
            var path = Path(name);
            try
            {
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
                        var fileInfo = new FileInfo(path);
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();
                            // if there are no more files or subdirectory in the directory... delete if
                            if (fileInfo.Directory.GetFiles().Count() == 0 && fileInfo.Directory.GetDirectories().Count() == 0)
                            {
                                fileInfo.Directory.Delete();
                            }
                        }
                    });
                }
            }
            catch (IOException ex)
            {
                throw ExceptionWithFileName(ex, path);
            }
        }

        private XNode Parsed(string name, byte[] data)
        {
            XDocument doc;
            if (data.Length == 0)
            {
                var rootName = new Normalized(name).AsString();
                if (rootName.ToLower().EndsWith(".xml"))
                {
                    rootName = rootName.Substring(0, rootName.Length - 4);
                }
                rootName = rootName.Substring(rootName.LastIndexOf("/"));
                rootName = rootName.TrimStart('/');
                doc =
                    new XDocument(
                        new XDeclaration("1.0", "UTF-8", "yes"),
                        new XElement(rootName)
                    );
            }
            else
            {
                try
                {
                    using (var reader = new StreamReader(new MemoryStream(data)))
                    {
                        doc = XDocument.Load(reader);
                    }
                }
                catch (XmlException ex)
                {
                    throw
                        new ApplicationException(
                            $"Cannot parse this content as XML: '{new TextOf(new InputOf(data), Encoding.UTF8).AsString()}'",
                            ex
                        );
                }
            }
            return doc;
        }

        private string Path(string name)
        {
            return new Normalized(System.IO.Path.Combine(this.root, name)).AsString();
        }

        private InvalidOperationException ExceptionWithFileName(Exception inner, string fileName)
        {
            return
                new InvalidOperationException(
                    $"Failed to access file '{fileName}'. {inner.Message}",
                    inner
                );
        }
    }
}
