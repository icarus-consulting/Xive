using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Xive.Hive;
using Xive.Mnemonic;
using Xive.Xocument;
using Xunit;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.IO;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Test.Hive
{
    public sealed class TextIndexTests
    {
        [Fact]
        public void AddsItems()
        {
            var idx = new TextIndex("test", new RamMemories());
            idx.Add("123");
            Assert.True(idx.Has("123"));
        }

        [Fact]
        public void RemovesFromIndex()
        {
            var idx = new TextIndex("test", new RamMemories());
            idx.Add("123");
            idx.Remove("123");
            Assert.False(idx.Has("123"));
        }

        [Fact]
        public void RemovesCells()
        {
            var mem = new RamMemories();
            var idx = new TextIndex("test", mem);
            idx.Add("123").Cell("xunit-test").Update(new InputOf("content"));
            idx.Remove("123");
            Assert.DoesNotContain($"test/123/xunit-test", mem.Data().Knowledge());
        }

        [Fact]
        public void RemovesXmls()
        {
            var mem = new RamMemories();
            var idx = new TextIndex("test", mem);
            idx.Add("123").Xocument("xunit-test").Modify(new Directives().Xpath("/xunit-test").Add("content").Set("boo"));
            idx.Remove("123");
            Assert.DoesNotContain($"test/123/xunit-test", mem.XML().Knowledge());
        }

        [Fact]
        public void FiltersItems()
        {
            var mem = new RamMemories();
            var idx = new TextIndex("test", mem);
            idx.Add("123");
            idx.Add("456");
            mem.Props("test", "123").Refined("works", "true");
            mem.Props("test", "456").Refined("works", "false");

            Assert.Equal(
                "test/456", idx.List(new IndexFilterOf(props => props.Value("works", "") == "false"))[0].Name()
            );
        }

        [Fact]
        public void CachesItems()
        {
            var mem = new RamMemories();
            var idx = new TextIndex("test", mem);
            idx.Add("123");

            new MemorizedXocument(
                $"test/hq/catalog.xml",
                mem
            ).Modify(
                new Directives().Xpath("/catalog/*").Remove()
            );
            Assert.True(idx.Has("123"));
        }

        [Fact]
        public void ReloadsCacheAfterAdd()
        {
            var mem = new RamMemories();
            var idx = new TextIndex("test", mem);
            idx.Add("123");

            new MemorizedXocument(
                $"test/hq/catalog.xml",
                mem
            ).Modify(
                new Directives().Xpath("/catalog/*").Remove()
            );

            idx.Add("456"); //trigger reloading from file by updating index

            Assert.False(idx.Has("123"));
        }

        [Fact]
        public void ReloadsCacheAfterRemove()
        {
            var mem = new RamMemories();
            var idx = new TextIndex("test", mem);
            idx.Add("123");

            new MemorizedXocument(
                $"test/hq/catalog.xml",
                mem
            ).Modify(
                new Directives().Xpath("/catalog/*").Remove()
            );

            idx.Add("456");
            idx.Remove("456"); //trigger reloading from file by updating index

            Assert.False(idx.Has("123"));
        }
    }
}
