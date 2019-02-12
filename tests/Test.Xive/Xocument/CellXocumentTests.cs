using Xive.Cell;
using Xive.Xocument;
using Xunit;
using Yaapii.Xambly;

namespace Xive.Test.Xocument
{
    public sealed class CellXocumentTests
    {
        [Fact]
        public void DeliversContent()
        {
            Assert.Equal(
                "<flash />",
                new CellXocument(
                    new RamCell("flash.xml"),
                    "flash.xml"
                ).Node().ToString()
            );
        }

        [Fact]
        public void UpdatesContent()
        {
            var xoc =
                new CellXocument(
                    new RamCell("flash.xml"),
                    "flash.xml"
                );
            
            xoc.Modify(
                new Directives()
                    .Xpath("/flash")
                    .Add("grandmaster").Set("scratch scratch")
            );

            Assert.Equal("scratch scratch", xoc.Value("/flash/grandmaster/text()", ""));
        }
    }
}
