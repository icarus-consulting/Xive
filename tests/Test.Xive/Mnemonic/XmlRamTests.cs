using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Xive.Cache;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Xml;

namespace Xive.Mnemonic.Test
{
    public sealed class XmlRamTests
    {
        [Fact]
        public void Memorizes()
        {
            var data = new XDocument(new XElement("root", new XText("1990's")));
            var mem = new XmlRamMemory();
            mem.Content("childhood", () => data);

            Assert.Equal(
                data,
                mem.Content(
                    "childhood",
                    () => throw new ApplicationException("Assumed to have memory"))
                );
        }

        [Fact]
        public void Updates()
        {
            var mem = new XmlRamMemory();
            mem.Content("childhood", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
            mem.Update("childhood", new XDocument(new XElement("root", new XElement("years", new XText("nothing")))));

            Assert.Equal(
                "nothing",
                new XMLCursor(
                    mem.Content(
                        "childhood",
                        () => throw new ApplicationException("Assumed to have memory")
                    )
                ).Values("/root/years/text()")[0]
            );
        }

        [Fact]
        public void RemovesIfEmpty()
        {
            var mem = new XmlRamMemory();
            mem.Content("childhood", () => new XDocument(new XElement("root", new XText("1980's"))));
            mem.Update("childhood", new XDocument(new XElement("root")));

            Assert.False(mem.Knows("childhood"));
        }

        [Fact]
        public void KnowledgeIsSeparatorInsensitive()
        {
            var mem = new XmlRamMemory();
            mem.Content("childhood\\subdir/file", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
            Assert.Equal(
                "childhood/subdir/file",
                new FirstOf<string>(mem.Knowledge()).Value()
            );
        }
    }
}
