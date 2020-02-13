using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Xive.Mnemonic.Test
{
    public sealed class LocalSyncPipeTests
    {
        [Fact]
        public void SyncsActions()
        {
            var pipe = new LocalSyncPipe();
            List<string> result = new List<string>();
            Parallel.For(0, 256, (i) =>
            {
                pipe.Flush("lock-this", () =>
                {
                    var expected = Guid.NewGuid().ToString();
                    result.Add(expected);
                    Assert.Equal(expected, result[result.Count - 1]);
                });
            });
        }
    }
}
