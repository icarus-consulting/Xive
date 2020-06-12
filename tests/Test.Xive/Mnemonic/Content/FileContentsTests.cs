using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Xive.Mnemonic;
using Xive.Mnemonic.Content;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Test.Mnemonic.Content
{
    public sealed class FileContentsTests
    {
        [Fact]
        public void DeliversBytes()
        {
            using (var dir = new TempDirectory())
            {
                var root = dir.Value().FullName;
                var subDir = Path.Combine(dir.Value().FullName, "a", "b", "c");
                Directory.CreateDirectory(subDir);
                File.WriteAllText(Path.Combine(subDir, "content.dat"), "Kßeiv");
                Assert.Equal(
                    new BytesOf("Kßeiv").AsBytes(),
                    new FileContents(root, new LocalSyncPipe())
                        .Bytes("a/b/c/content.dat", () => new byte[0])
                );
            }
        }

        [Fact]
        public void DeliversAbsentBytes()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Equal(
                    0x13,
                    new FileContents(
                        dir.Value().FullName,
                        new LocalSyncPipe()
                    )
                    .Bytes("a/b/c/content.dat", () => new byte[1] { 0x13 })[0]
                );
            }
        }

        [Fact]
        public void KnowsAbsentBytesAfterFirstRead()
        {
            using (var dir = new TempDirectory())
            {
                var mem = new FileContents(dir.Value().FullName, new LocalSyncPipe());
                mem.Bytes("a/b/c.dat", () => new byte[1] { 0x13 }).ToString();

                Assert.Equal(
                    new byte[1] { 0x13 },
                    mem.Bytes("a/b/c.dat", () => new byte[0])
                );
            }
        }

        [Fact]
        public void DeliversXNodeFromBytes()
        {
            using (var dir = new TempDirectory())
            {
                var root = dir.Value().FullName;
                var subDir = Path.Combine(dir.Value().FullName, "a", "b", "c");
                Directory.CreateDirectory(subDir);
                File.WriteAllText(
                    Path.Combine(subDir, "content.xml"),
                    new XDocument(new XElement("elem", "content")).ToString()
                );

                Assert.Equal(
                    "<elem>content</elem>",
                    new FileContents(root, new LocalSyncPipe())
                        .Xml("a/b/c/content.xml", () => new XDocument()).ToString()
                );
            }
        }

        [Fact]
        public void DeliversAbsentXNode()
        {
            using (var dir = new TempDirectory())
            {
                var root = dir.Value().FullName;
                Assert.Equal(
                    "<elem>content</elem>",
                    new FileContents(root, new LocalSyncPipe())
                        .Xml("a/b/c.xml", () => new XDocument(new XElement("elem", "content"))).ToString()
                );
            }
        }

        [Fact]
        public void KnowsAbsentXmlBytesAfterFirstRead()
        {
            using (var dir = new TempDirectory())
            {
                var root = dir.Value().FullName;
                var mem = new FileContents(root, new LocalSyncPipe());
                mem.Xml("a/b/c.xml", () => new XDocument(new XElement("elem", "content"))).ToString();

                Assert.Equal(
                    "<elem>content</elem>",
                    new TextOf(mem.Bytes("a/b/c.xml", () => new byte[0])).AsString()
                );
            }
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
