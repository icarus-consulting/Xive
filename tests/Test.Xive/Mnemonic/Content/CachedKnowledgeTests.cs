using System.Collections.Concurrent;
using System.Collections.Generic;
using Xunit;
using Yaapii.Atoms.Enumerable;

namespace Xive.Mnemonic.Content.Test
{
    public sealed class CachedKnowledgeTests
    {
        [Fact]
        public void DeliversKnowledge()
        {
            Assert.Contains(
                "you don't know Jack",
                new CachedKnowledge(
                    new RamContents(
                        new KeyValuePair<string, byte[]>("you don't know Jack", new byte[1] { 0x01 })
                    )
                ).Knowledge()
            );
        }

        [Fact]
        public void CachesKnowledge()
        {
            var mem =
                new ConcurrentDictionary<string, byte[]>(
                    new ManyOf<KeyValuePair<string, byte[]>>(
                        new KeyValuePair<string, byte[]>("you don't know Jack", new byte[1] { 0x01 })
                    )
                );
            var contents = 
                new CachedKnowledge(
                    new RamContents(mem)
                );
            contents.Knowledge(); //first read
            byte[] unused;
            mem.TryRemove("you don't know Jack", out unused); //then remove

            Assert.Contains(
                "you don't know Jack",
                contents.Knowledge()
            );
        }

        [Fact]
        public void RefreshesKnowledgeOnUpdate()
        {
            var mem =
                new ConcurrentDictionary<string, byte[]>(
                    new ManyOf<KeyValuePair<string, byte[]>>(
                        new KeyValuePair<string, byte[]>("you don't know Jack", new byte[1] { 0x01 })
                    )
                );
            var contents =
                new CachedKnowledge(
                    new RamContents(mem)
                );
            contents.Knowledge(); //first read
            byte[] unused;
            mem.TryRemove("you don't know Jack", out unused); //then remove

            Assert.Contains(
                "you don't know Jack",
                contents.Knowledge()
            );
        }
    }
}
