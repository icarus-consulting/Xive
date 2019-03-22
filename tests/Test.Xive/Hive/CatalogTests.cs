//MIT License

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

using System.Collections.Generic;
using Xunit;
using Yaapii.Xambly;
using Xive.Cell;
using Xive.Comb;
using Xive.Xocument;
using System.IO;

namespace Xive.Hive.Test
{
    public sealed class CatalogTests
    {
        [Theory]
        [InlineData("123")]
        [InlineData("456")]
        public void AddsEntry(string id)
        {
            var memory = new Dictionary<string, MemoryStream>();
            var catalog =
                new Catalog(
                    "my-hive",
                    new SimpleComb(
                        "hq",
                        cell => new RamCell(cell, memory),
                        (cellName, cell) => new CellXocument(cell, cellName)
                    )
                );

            catalog.Create("123");
            catalog.Create("456");

            Assert.Contains(
                id,
                catalog.List("'*'")
            );
        }

        [Fact]
        public void RemovesEntry()
        {
            var memory = new Dictionary<string, MemoryStream>();
            var catalog =
                new Catalog(
                    "my-hive",
                    new SimpleComb(
                        "hq",
                        cell => new RamCell(cell, memory),
                        (cellName, cell) => new CellXocument(cell, cellName)
                    )
                );

            catalog.Update("123", new Directives());
            catalog.Remove("123");

            Assert.DoesNotContain(
                "123",
                catalog.List("*")
            );
        }

        [Fact]
        public void UpdatesEntry()
        {
            var memory = new Dictionary<string, MemoryStream>();
            var catalog =
                new Catalog(
                    "my-hive",
                    new SimpleComb(
                        "hq",
                        cell => new RamCell(cell, memory),
                        (cellName, cell) => new CellXocument(cell, cellName)
                    )
                );

            catalog.Create("123");
            catalog.Update(
                "123", 
                new Directives()
                    .Add("todo")
                    .Set("code some stuff")
            );

            Assert.Contains(
                "123",
                catalog.List("todo[text()='code some stuff']")
            );
        }
    }
}
