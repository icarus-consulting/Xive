using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xive.Test;
using Xunit;

namespace Xive.Xocument
{
    public sealed class SyncXocumentTests
    {
        [Fact]
        public void DeliversParallel()
        {
            var accesses = 0;
            var xoc =
                new SyncXocument("synced",
                    new FkXocument(() =>
                    {
                        accesses++;
                        Assert.Equal(1, accesses);
                        accesses--;
                        return
                            new XDocument(
                                new XElement("synced", new XText("here"))
                            );
                    })
                );

            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                xoc.Value("/synced/text()", "");
            });
        }
    }
}
