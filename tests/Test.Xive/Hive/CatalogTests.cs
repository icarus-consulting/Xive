using System.Collections.Generic;
using Xunit;
using Yaapii.Xambly;
using Xive.Cell;
using Xive.Comb;
using Xive.Xocument;

namespace Xive.Hive.Test
{
    public sealed class CatalogTests
    {
        [Theory]
        [InlineData("123")]
        [InlineData("456")]
        public void AddsEntry(string id)
        {
            var memory = new Dictionary<string, byte[]>();
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
            var memory = new Dictionary<string, byte[]>();
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
            var memory = new Dictionary<string, byte[]>();
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
