using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xive.Mnemonic;
using Xive.Xocument;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public sealed class TextIndexTests
    {
        [Fact]
        public void IsThreadSafe()
        {
            var idx = new TextIndex("test", new RamMnemonic());

            Parallel.For(0, 10000, i =>
            {
                idx.Add(i.ToString());
                idx.List();
            });

            Assert.Equal(
                10000,
                idx.List().Count
            );
        }

        [Fact]
        public void AddsItems()
        {
            var idx = new TextIndex("test", new RamMnemonic());
            idx.Add("123");
            Assert.True(idx.Has("123"));
        }

        [Fact]
        public void RemovesFromIndex()
        {
            var idx = new TextIndex("test", new RamMnemonic());
            idx.Add("123");
            idx.Remove("123");
            Assert.False(idx.Has("123"));
        }

        [Fact]
        public void RemovesCells()
        {
            var mem = new RamMnemonic();
            var idx = new TextIndex("test", mem);
            idx.Add("123").Cell("xunit-test").Update(new InputOf("content"));
            idx.Remove("123");
            Assert.DoesNotContain($"test/123/xunit-test", mem.Contents().Knowledge());
        }

        [Fact]
        public void RemovesXmls()
        {
            var mem = new RamMnemonic();
            var idx = new TextIndex("test", mem);
            idx.Add("123").Xocument("xunit-test").Modify(new Directives().Xpath("/xunit-test").Add("content").Set("boo"));
            idx.Remove("123");
            Assert.DoesNotContain($"test/123/xunit-test", mem.Contents().Knowledge());
        }

        [Fact]
        public void FiltersItems()
        {
            var mem = new RamMnemonic();
            var idx = new TextIndex("test", mem);
            idx.Add("123");
            idx.Add("456");
            idx.Add("789");
            mem.Props("test", "123").Refined("id", "123");
            mem.Props("test", "123").Refined("works", "true");
            mem.Props("test", "456").Refined("id", "456");
            mem.Props("test", "456").Refined("works", "false");
            mem.Props("test", "789").Refined("id", "789");
            mem.Props("test", "789").Refined("works", "false");

            Assert.Equal(
                1,
                idx.List(
                    new IndexFilterOf(props => props.Value("works", "") == "false"),
                    new IndexFilterOf(props => props.Value("id", "") != "456")
                ).Count
            );
        }

        [Fact]
        public void FindsSingleItem()
        {
            var mem = new RamMnemonic();
            var idx = new TextIndex("test", mem);
            idx.Add("123")
                .Cell("zäll")
                .Update(new InputOf("condänd"));

            Assert.Equal(
                "condänd",
                new TextOf(
                    new InputOf(
                        idx.Comb("123")
                            .Cell("zäll")
                            .Content()
                    )
                ).AsString()
            );
        }

        [Fact]
        public void RejectsUnknownSingleItem()
        {
            var mem = new RamMnemonic();
            var idx = new TextIndex("test", mem);

            Assert.Throws<ArgumentException>(() =>
                idx.Comb("123")
            );
        }

        [Fact]
        public void CachesItems()
        {
            var mem = new RamMnemonic();
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
        public void WorksWithLargeData()
        {
            var mem = new RamMnemonic();
            var idx = new TextIndex("test", mem);
            var random = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var number = random.Next();
                while (idx.Has(number.ToString()))
                {
                    number = random.Next();
                }
                idx.Add(number.ToString());
            }
            Assert.Equal(
                1000,
                idx.List(new IndexFilterOf(props => true)).Count
            );
        }

        [Fact]
        public void RemovesWithLargeData()
        {
            var mem = new RamMnemonic();
            var idx = new TextIndex("test", mem);
            var random = new Random();
            var numbers = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                var number = random.Next();
                while (idx.Has(number.ToString()))
                {
                    number = random.Next();
                }
                numbers.Add(number.ToString());
                idx.Add(number.ToString());
            }
            for (int i = 0; i < 1000; i++)
            {
                idx.Remove(numbers[i]);
            }
            Assert.Equal(0, idx.List().Count);
        }
    }
}
