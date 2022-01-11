//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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
using System.Threading.Tasks;
using System.Xml.Linq;
using Xive.Mnemonic;
using Xive.Test;
using Xunit;
using Yaapii.Xambly;

namespace Xive.Xocument.Test
{
    public sealed class SyncXocumentTests
    {
        [Fact]
        public void ReadsExclusive()
        {
            var accesses = 0;
            var syncGate = new LocalSyncPipe();
            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
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
                        }),
                        syncGate
                    );
                xoc.Value("/synced/text()", "");
            });
        }

        [Fact]
        public void ReadsValues()
        {
            var xoc =
                new SyncXocument("synced",
                    new RamXocument("synced"),
                    new LocalSyncPipe()
                );
            xoc.Modify(new Directives().Xpath("/synced").Set("here"));

            Assert.Equal(
                "here",
                xoc.Values("/synced/text()")[0]
            );
        }

        [Fact]
        public void ModifiesExclusive()
        {
            var syncGate = new LocalSyncPipe();
            var mem = new RamMnemonic();
            Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
            {
                var content = Guid.NewGuid().ToString();
                var synced =
                    new SyncXocument("xoc",
                        new MemorizedXocument("xoc", mem),
                        syncGate
                    );

                synced.Modify(new Directives().Xpath("/xoc").Set(content));
                Assert.NotEmpty(synced.Value("/xoc/text()", ""));
            });
        }
    }
}
