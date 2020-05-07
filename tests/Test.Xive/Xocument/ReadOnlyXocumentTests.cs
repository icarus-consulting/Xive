using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Xocument.Test
{
    public sealed class ReadOnlyXocumentTests
    {
        [Fact]
        public void ThrowsOnModify()
        {
            var xoc = new ReadOnlyXocument(new XMLSlice("<root></root>"));
            Assert.Throws<InvalidOperationException>(() =>
                xoc.Modify(new Directives())
            );
        }
    }
}
