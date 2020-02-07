using System;
using System.IO;
using Xive.Cache;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Test.Mnemonic
{
    public sealed class DataInFilesTests
    {
        [Fact]
        public void Memorizes()
        {
            using (var temp = new TempDirectory())
            {
                var data = new MemoryStream();
                new InputOf("1980's").Stream().CopyTo(data);
                var mem = new DataInFiles(temp.Value().FullName);
                mem.Content("childhood", () => data);

                Assert.Equal(
                    "1980's",
                    new TextOf(
                        new InputOf(
                            mem.Content(
                                "childhood",
                                () => throw new ApplicationException("Assumed to have memory"))
                        )
                    ).AsString()
                );
            }
        }

        [Fact]
        public void Updates()
        {
            using (var temp = new TempDirectory())
            {
                var data = new MemoryStream();
                new InputOf("1980's").Stream().CopyTo(data);
                var mem = new DataInFiles(temp.Value().FullName);
                mem.Update("childhood", data);
                data.Seek(0, SeekOrigin.Begin);
                data.SetLength(0);
                new InputOf("nothing").Stream().CopyTo(data);
                mem.Update("childhood", data);

                Assert.Equal(
                    "nothing",
                    new TextOf(
                        new InputOf(
                            mem.Content(
                                "childhood",
                                () => throw new ApplicationException("Assumed to have memory"))
                        )
                    ).AsString()
                );
            }
        }

        [Fact]
        public void RemovesIfEmpty()
        {
            using (var temp = new TempDirectory())
            {
                var data = new MemoryStream();
                new InputOf("1980's").Stream().CopyTo(data);
                var mem = new DataInFiles(temp.Value().FullName);
                mem.Update("childhood", data);
                data.Seek(0, SeekOrigin.Begin);
                data.SetLength(0);
                mem.Update("childhood", data);

                Assert.False(
                    mem.Knows("childhood")
                );
            }
        }
    }
}
