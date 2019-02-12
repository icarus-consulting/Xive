using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument.Test
{
    public sealed class SimpleXocumentTests
    {
        [Theory]
        [InlineData("A")]
        [InlineData("B")]
        public void ReadsContent(string expected)
        {
            Assert.Contains(
                expected,
                new SimpleXocument(
                    new XMLQuery(
                        new InputOf("<root><item>A</item><item>B</item></root>")
                    ).Node()
                ).Values("//item/text()")
            );
        }

        [Fact]
        public void ModifiesContent()
        {
            var xoc =
                new SimpleXocument(
                    new XMLQuery(
                        new InputOf("<root><item>A</item><item>A</item></root>")
                    ).Node()
                );
            xoc.Modify(
                    new Directives()
                        .Xpath("//item")
                        .Set("B")
                );

            Assert.Contains(
                "B",
                xoc.Values("//item/text()")
            );
        }

        [Fact]
        public void FindsNodes()
        {
            var xoc =
                new SimpleXocument(
                    new XMLQuery(
                        new InputOf(
                            "<root><item>A</item><item>B</item></root>"
                        )
                    ).Node()
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
                new SimpleXocument(
                    new XMLQuery(
                        new InputOf(
                            "<root><item>A</item></root>"
                        )
                    ).Node()
                );

            Assert.Equal(
                "A",
                xoc.Nodes("//item")[0].Values("text()")[0]
            );
        }
    }
}
