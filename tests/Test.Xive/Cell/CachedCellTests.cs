using System.Collections.Generic;
using Xive.Test;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Cell.Test
{
    public sealed class CachedCellTests
    {
        [Fact]
        public void ReadsOnce()
        {
            var cache = new Dictionary<string, byte[]>();
            int reads = 0;
            var cell =
                new CachedCell(
                    new FkCell(
                        content => { },
                        () =>
                        {
                            reads++;
                            return new byte[0];
                        }
                    ),
                    "cached",
                    cache
                );

            cell.Content();
            cell.Content();

            Assert.Equal(1, reads);
        }

        [Fact]
        public void FillsCache()
        {
            var cache = new Dictionary<string, byte[]>();
            new CachedCell(
                new FkCell(
                    content => { },
                    () => 
                        new BytesOf(
                            new InputOf("Some content")
                        ).AsBytes()
                ),
                "cached",
                cache
            ).Content();

            Assert.Equal(
                "Some content",
                new TextOf(
                    new InputOf(cache["cached"])
                ).AsString()
            );
        }

        [Fact]
        public void UpdatesCache()
        {
            var cache = new Dictionary<string, byte[]>();
            var cell =
                new CachedCell(
                    new FkCell(
                        content => {  },
                        () => new BytesOf(new InputOf("Old content")).AsBytes()
                    ),
                    "cached",
                    cache
                );
            cell.Content();
            cell.Update(new InputOf("New content"));

            Assert.Equal(
                "New content",
                new TextOf(
                    new InputOf(cache["cached"])
                ).AsString()
            );
        }
    }
}
