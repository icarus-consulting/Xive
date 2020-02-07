using System;
using System.IO;
using Xive.Cache;
using Xunit;
using Yaapii.Atoms.IO;

namespace Xive.Test.Mnemonic
{
    public sealed class DataRamTests
    {
        [Fact]
        public void Memorizes()
        {
            var data = new MemoryStream();
            var mem = new DataRam();
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
            var data = new MemoryStream();
            new InputOf("1980's").Stream().CopyTo(data);
            var mem = new DataRam();
            mem.Update("childhood", data);

            Assert.Equal(
                data,
                mem.Content(
                    "childhood",
                    () => throw new ApplicationException("Assumed to have memory"))
                );
        }

        [Fact]
        public void RemovesIfEmpty()
        {
            var data = new MemoryStream();
            new InputOf("1980's").Stream().CopyTo(data);
            var mem = new DataRam();
            mem.Update("childhood", data);
            mem.Update("childhood", new MemoryStream());

            Assert.False(
                mem.Knows("childhood")
            );
        }
    }
}
