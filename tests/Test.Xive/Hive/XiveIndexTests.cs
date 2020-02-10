using Xive.Hive;
using Xive.Mnemonic;
using Xunit;

namespace Xive.Test.Hive
{
    public sealed class XiveIndexTests
    {
        [Fact]
        public void AddsItems()
        {
            var idx = new XiveIndex("test", new RamMemories(), new SyncGate());
            idx.Add("123");
            Assert.True(idx.Has("123"));
        }

        [Fact]
        public void RemovesItems()
        {
            var idx = new XiveIndex("test", new RamMemories(), new SyncGate());
            idx.Add("123");
            idx.Remove("123");
            Assert.False(idx.Has("123"));
        }

        [Fact]
        public void FiltersItems()
        {
            var mem = new RamMemories();
            var idx = new XiveIndex("test", mem, new SyncGate());
            idx.Add("123");
            idx.Add("456");
            mem.Props("test", "123").Refined("works", "true");
            mem.Props("test", "456").Refined("works", "false");

            Assert.Equal(
                "test/456", idx.List(new IndexFilterOf(props => props.Value("works", "") == "false"))[0].Name()
            );
        }
    }
}
