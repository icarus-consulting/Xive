using System;
using System.IO;
using System.Xml.Linq;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xml;

namespace Xive.Test.Mnemonic
{
    public sealed class XmlInFilesTests
    {
        [Fact]
        public void Memorizes()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new XmlInFiles(temp.Value().FullName);
                mem.Content("childhood", () => new XDocument(new XElement("root", new XText("1980's"))));

                Assert.Equal(
                    "1980's",
                    new XMLCursor(
                        mem.Content(
                            "childhood",
                            () => throw new ApplicationException("Assumed to have memory")
                        )
                    ).Values("/root/text()")[0]
                );
            }
        }

        [Fact]
        public void Updates()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new XmlInFiles(temp.Value().FullName);
                mem.Content("childhood", () => new XDocument(new XElement("root", new XText("1980's"))));
                mem.Update("childhood", new XDocument(new XElement("root", new XText("nothing"))));

                Assert.Equal(
                    "nothing",
                    new XMLCursor(
                        mem.Content(
                            "childhood",
                            () => throw new ApplicationException("Assumed to have memory")
                        )
                    ).Values("/root/text()")[0]
                );
            }
        }

        [Fact]
        public void RemovesIfEmpty()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new XmlInFiles(temp.Value().FullName);
                mem.Content("childhood", () => new XDocument(new XElement("root", new XText("1980's"))));
                mem.Update("childhood", new XDocument(new XElement("root")));

                Assert.False(mem.Knows("childhood"));
            }
        }
    }
}
