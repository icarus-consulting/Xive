//MIT License

//Copyright (c) 2020 ICARUS Consulting GmbH

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.IO;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Mnemonic.Test
{
    public sealed class DataInFilesTests
    {
        [Fact]
        public void Memorizes()
        {
            using (var temp = new TempDirectory())
            {
                var data =
                    new BytesOf(
                        new InputOf("1980's")
                    ).AsBytes();

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
                var data = new BytesOf(new InputOf("1980's")).AsBytes();
                var mem = new DataInFiles(temp.Value().FullName);
                mem.Update("childhood", data);
                data = new BytesOf(new InputOf("nothing")).AsBytes();
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
        public void DeliversKnowledge()
        {
            using (var temp = new TempDirectory())
            {
                var data =
                    new BytesOf(
                        new InputOf("1980's")
                    ).AsBytes();
                var mem = new DataInFiles(temp.Value().FullName);
                mem.Content("childhood", () => data);
                Assert.Contains(
                    "childhood",
                    mem.Knowledge()
                );
            }
        }

        [Fact]
        public void RemovesIfEmpty()
        {
            using (var temp = new TempDirectory())
            {
                var data =
                    new BytesOf(
                        new InputOf("1980's")
                    ).AsBytes();
                var mem = new DataInFiles(temp.Value().FullName);
                mem.Update("childhood", data);
                mem.Update("childhood", new byte[0]);

                Assert.False(
                    mem.Knows("childhood")
                );
            }
        }

        [Fact]
        public void KnowledgeIsSeparatorInsensitive()
        {
            using (var temp = new TempDirectory())
            {
                var data =
                    new BytesOf(
                        new InputOf("1980's")
                    ).AsBytes();
                var mem = new DataInFiles(temp.Value().FullName);
                mem.Content("childhood\\subdir/file", () => data);
                Assert.Equal(
                    "childhood/subdir/file",
                    new FirstOf<string>(mem.Knowledge()).Value()
                );
            }
        }

        [Fact]
        public void FindsFileCaseInsensitive()
        {
            using (var temp = new TempDirectory())
            {
                var data =
                    new BytesOf(
                        new InputOf("1980's")
                    ).AsBytes();
                var mem = new DataInFiles(temp.Value().FullName);
                mem.Update("file/with/dir.txt", data);
                Assert.Equal(
                    data,
                    mem.Content("File/wiTh/Dir.Txt", () => throw new Exception())
                );
            }
        }
    }
}
