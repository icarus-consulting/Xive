﻿//MIT License

//Copyright (c) 2019 ICARUS Consulting GmbH

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

using System.IO;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;

namespace Xive.Cell.Test
{
    public sealed class RamCellTests
    {
        [Fact]
        public void InitializesWithMemory()
        {
            using (var cell = new RamCell("my-cell", new MemoryStream(new byte[1] { 0x00 })))
            {
                Assert.True(
                    cell.Content()[0]
                        .Equals(0x00)
                );
            }
        }

        [Fact]
        public void ResetsStreamAfterUpdate()
        {
            using (var cell = new RamCell("my-cell"))
            {
                var content = new InputOf("its so hot outside");
                cell.Update(content);
                Assert.Equal(0, content.Stream().Position);
            }
        }

        [Fact]
        public void CanUpdate()
        {
            using (var cell = new RamCell("my-cell"))
            {
                cell.Update(new InputOf("its so hot outside"));
                Assert.Equal(
                    "its so hot outside",
                    new TextOf(cell.Content()).AsString()
                );
            }
        }
    }
}
