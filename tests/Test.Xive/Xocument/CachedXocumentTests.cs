using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Xive.Xocument;
using Xunit;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Test.Xocument
{
    public sealed class CachedXocumentTests
    {
        [Fact]
        public void FillsCacheWithContent()
        {
            var cache = new Dictionary<string, XNode>();
            new CachedXocument(
                "buffered.xml",
                new SimpleXocument("buffered"),
                cache
            ).Node();

            Assert.Equal(
                "<buffered />",
                cache["buffered.xml"].ToString()
            );
        }

        [Fact]
        public void ReadsContentFromCache()
        {
            var reads = 0;
            var cache = new Dictionary<string, XNode>();
            var xoc =
                new CachedXocument(
                    "buffered.xml",
                    new FkXocument(() =>
                    {
                        reads++;
                        return new XDocument(new XElement("buffered"));
                    }),
                    cache
                );
            xoc.Node();
            xoc.Node();

            Assert.Equal(1, reads);
        }

        [Fact]
        public void UpdatesCache()
        {
            var cache = new Dictionary<string, XNode>();
            var xoc =
                new CachedXocument(
                    "buffered.xml",
                    new SimpleXocument("buffered"),
                    cache
                );
            xoc.Node();
            xoc.Modify(
                new Directives()
                    .Xpath("/buffered")
                    .Set("10 Minutes")
            );

            Assert.Equal(
                "10 Minutes",
                new XMLQuery(cache["buffered.xml"]).Values("/buffered/text()")[0]
            );
        }
    }
}
