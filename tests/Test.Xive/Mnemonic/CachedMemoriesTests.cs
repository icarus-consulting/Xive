using System;
using System.IO;
using System.Xml.Linq;
using Xive.Cell;
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
            var mem = new RamMemories();
            var cache = new CachedMemories(mem);
            var data = new MemoryStream();
            new InputOf("splashy").Stream().CopyTo(data);
            data.Seek(0, SeekOrigin.Begin);

            cache.Data().Content("cashy", () => data); //read 1
            mem.Data().Update("cashy", new MemoryStream());

            Assert.Equal(
                "splashy",
                new TextOf(
                    new InputOf(
                        cache.Data().Content("cashy", () => throw new ApplicationException($"Assumed to have memory"))
                    )
                ).AsString()
            );

        }

        [Fact]
        public void CachesXml()
        {
            var data = (XNode)new XDocument(new XElement("root", new XText("potato")));
            var mem = new RamMemories();
            var cache = new CachedMemories(mem);

            cache.XML().Content("cashy", () => data); //read 1
            mem.XML().Update("cashy", (XNode)new XDocument(new XElement("root", new XText(""))));

            Assert.Contains(
                "potato",
                new XMLCursor(
                    cache.XML().Content("cashy", () => throw new ApplicationException($"Assumed to have memory"))
                ).Values("/root/text()")
            );
        }

        [Fact]
        public void BlacklistsItems()
        {
            var mem = new RamMemories();
            var cache = new CachedMemories(mem, "a/*/blacklisted/*");
            var cell =
                    new MemorizedCell(
                        "a/file\\which/is\\blacklisted/data.dat",
                        cache
                    );

            cell.Content();
            mem.Data().Update("a/file\\which/is\\blacklisted/data.dat", new MemoryStream(new byte[128]));

            Assert.False(cache.Data().Knows("a/file\\which/is\\blacklisted/data.dat"));
        }

        [Fact]
        public void DoesNotCacheOversized()
        {
            var mem = new RamMemories();
            var cache = new CachedMemories(mem, 4);
            var cell =
                new MemorizedCell(
                    "a/file/which/is/oversized",
                    cache
                );
            cell.Update(new InputOf(new byte[128]));
            cell.Content();
            mem.Data().Update("a/file/which/is/oversized", new MemoryStream());

            Assert.True(cache.Data().Content("a/file/which/is/oversized", () => new MemoryStream()).Length == 0);
        }
    }
}
