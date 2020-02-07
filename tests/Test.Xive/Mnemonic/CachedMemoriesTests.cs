using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;
using Xive.Cache;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xml;

namespace Xive.Test.Mnemonic
{
    public sealed class CachedMemoriesTests
    {
        [Fact]
        public void CachesData()
        {
            var data = new MemoryStream();
            new InputOf("splashy").Stream().CopyTo(data);
            data.Seek(0, SeekOrigin.Begin);
            var core = new ConcurrentDictionary<string, MemoryStream>();
            core.AddOrUpdate("cashy", data, (key, current) => data);

            var mem =
                new CachedMemories(
                    new SimpleMemories(
                        new XmlRam(),
                        new DataRam(core)
                    )
                );

            mem.Data().Content("cashy", () => data); //read 1
            core.TryRemove("cashy", out data);

            Assert.Equal(
                "splashy",
                new TextOf(
                    new InputOf(
                        mem.Data().Content("cashy", () => throw new ApplicationException($"Assumed to have memory"))
                    )
                ).AsString()
            );

        }

        [Fact]
        public void CachesXml()
        {
            var data = (XNode)new XDocument(new XElement("root", new XText("potato")));
            var core = new ConcurrentDictionary<string, XNode>();
            core.AddOrUpdate("mashy", data, (key, current) => data);

            var mem =
                new CachedMemories(
                    new SimpleMemories(
                        new XmlRam(core),
                        new DataRam()
                    )
                );

            mem.XML().Content("mashy", () => data); //read 1
            core.TryRemove("mashy", out data);

            Assert.Contains(
                "potato",
                new XMLCursor(
                    mem.XML().Content("mashy", () => throw new ApplicationException($"Assumed to have memory"))
                ).Values("/root/text()")
            );

        }
    }
}
