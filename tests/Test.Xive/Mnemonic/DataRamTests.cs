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
