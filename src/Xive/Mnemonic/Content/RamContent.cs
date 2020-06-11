using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic
{
    public sealed class RamContent : IContent
    {
        private readonly ConcurrentDictionary<string, byte[]> mem;

        public RamContent(params KeyValuePair<string, byte[]>[] bytes) : this(new ConcurrentDictionary<string, byte[]>(bytes))
        { }

        public RamContent(ConcurrentDictionary<string, byte[]> bytes)
        {
            this.mem = bytes;
        }

        public byte[] Bytes(string name, Func<byte[]> ifAbsent)
        {
            name = new Normalized(name).AsString();
            var result = this.mem.GetOrAdd(name, (key) => ifAbsent());
            return result;
        }

        public IList<string> Knowledge()
        {
            return new ListOf<string>(this.mem.Keys);
        }

        public void UpdateBytes(string name, byte[] data)
        {
            name = new Normalized(name).AsString();
            if (data.Length == 0)
            {
                byte[] removed;
                this.mem.TryRemove(name, out removed);
            }
            else
            {
                this.mem.AddOrUpdate(name, data, (currentName, currentContent) => data);
            }
        }

        public void UpdateXml(string name, XNode xml)
        {
            this.UpdateBytes(
                name,
                new BytesOf(
                    Encoding.UTF8.GetBytes(xml.ToString())
                ).AsBytes()
            );
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            name = new Normalized(name).AsString();
            XNode result;
            if (!this.mem.Keys.Contains(name))
            {
                result = ifAbsent();
                if (!result.Document.Root.IsEmpty)
                {
                    UpdateXml(name, result);
                }
            }
            else
            {
                result = Parsed(name, this.Bytes(name, () => throw new ApplicationException($"Internal error, assumend to never access ifAbsent() method here.")));
            }
            return result;
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
    }
}
