using System.Collections.Generic;
using System.Xml.Linq;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic.Content.Test
{
    public sealed class RamContentTests
    {
        [Fact]
        public void DeliversBytes()
        {
            Assert.Equal(
                0x13,
                new RamContents(
                    new KeyValuePair<string, byte[]>("a/b/c.dat", new byte[1] { 0x13 })
                ).Bytes("a/b/c.dat", () => new byte[0])[0]
            );
        }

        [Fact]
        public void DeliversAbsentBytes()
        {
            Assert.Equal(
                0x13,
                new RamContents()
                    .Bytes("a/b/c.dat", () => new byte[1] { 0x13 })[0]
            );
        }

        [Fact]
        public void KnowsAbsentBytesAfterFirstRead()
        {
            var mem = new RamContents();
            mem.Bytes("a/b/c.dat", () => new byte[1] { 0x13 }).ToString();

            Assert.Equal(
                new byte[1] { 0x13 },
                mem.Bytes("a/b/c.dat", () => new byte[0])
            );
        }

        [Fact]
        public void DeliversXNodeFromBytes()
        {
            Assert.Equal(
                "<elem>content</elem>",
                new RamContents(
                    new KeyValuePair<string, byte[]>("a/b/c.xml", new BytesOf(new XDocument(new XElement("elem", "content")).ToString()).AsBytes())
                ).Xml("a/b/c.xml", () => new XDocument()).ToString()
            );
        }

        [Fact]
        public void DeliversAbsentXNode()
        {
            Assert.Equal(
                "<elem>content</elem>",
                new RamContents()
                    .Xml("a/b/c.xml", () => new XDocument(new XElement("elem", "content"))).ToString()
            );
        }

        [Fact]
        public void KnowsAbsentXmlBytesAfterFirstRead()
        {
            var mem = new RamContents();
            mem.Xml("a/b/c.xml", () => new XDocument(new XElement("elem", "content"))).ToString();

            Assert.Equal(
                "<elem>content</elem>",
                new TextOf(mem.Bytes("a/b/c.xml", () => new byte[0])).AsString()
            );
        }

        [Fact]
        public void NormalizesSlashes()
        {
            var mem = new RamContents();
            mem.Xml(@"childhood\subdir/file", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
            Assert.Equal(
                @"childhood/subdir/file",
                new FirstOf<string>(mem.Knowledge()).Value()
            );
        }

        [Fact]
        public void PreservesCase()
        {
            var mem = new RamContents();
            mem.Xml(@"BIG\subdir/file", () => new XDocument(new XElement("root", new XElement("years", new XText("1980's")))));
            Assert.Equal(
                "BIG/subdir/file",
                new FirstOf<string>(mem.Knowledge()).Value()
            );
        }
    }
}
