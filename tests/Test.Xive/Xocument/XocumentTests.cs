using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Xambly;
using Xive.Cell;

namespace Xive.Xocument.Test
{
    public sealed class XocumentTests
    {
        [Theory]
        [InlineData("A")]
        [InlineData("B")]
        public void ReadsContent(string expected)
        {
            var cell =
                new RamCell(
                    "my-cell",
                    new BytesOf(
                        new InputOf(
                            "<root><item>A</item><item>B</item></root>"
                        )
                    ).AsBytes()
                );

            Assert.Contains(
                expected,
                new XocumentOf(cell).Values("//item/text()")
            );
        }

        [Fact]
        public void ModifiesContent()
        {
            var xoc =
                new XocumentOf(
                    new RamCell(
                        "my-cell",
                        new BytesOf(
                            new InputOf(
                                "<root><item>A</item><item>A</item></root>"
                            )
                        ).AsBytes()
                    )
                );

            xoc.Modify(new Directives().Xpath("//item").Set("B"));

            Assert.Contains(
                "B",
                xoc.Values("//item/text()")
            );
        }

        [Fact]
        public void FindsNodes()
        {
            var xoc =
                new XocumentOf(
                    new RamCell(
                        "my-cell",
                        new BytesOf(
                            new InputOf(
                                "<root><item>A</item><item>B</item></root>"
                            )
                        ).AsBytes()
                    )
                );

            Assert.Equal(
                1,
                xoc.Nodes("//item[text() = 'A']").Count
            );
        }

        [Fact]
        public void HasNodeContent()
        {
            var xoc =
                new XocumentOf(
                    new RamCell(
                        "my-cell",
                        new BytesOf(
                            new InputOf(
                                "<root><item>A</item></root>"
                            )
                        ).AsBytes()
                    )
                );

            Assert.Equal(
                "A",
                xoc.Nodes("//item")[0].Values("text()")[0]
            );
        }
    }
}
