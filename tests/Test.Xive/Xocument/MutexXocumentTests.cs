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

using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xive.Cell;
using Xive.Test;
using Xunit;
using Yaapii.Xambly;

namespace Xive.Xocument.Test
{
    public sealed class MutexXocumentTests
    {
        [Fact]
        public void DeliversParallel()
        {
            var accesses = 0;
            var xoc =
                new MutexXocument("synced",
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

        [Fact]
        public void WorksParallel()
        {
            var cell = new RamCell();
            Parallel.For(0, Environment.ProcessorCount << 4, (current) =>
            {
                var content = "CONTENT " + Guid.NewGuid().ToString();
                using (var mutexed = new MutexXocument(cell.Name(), new CellXocument(cell, "xoc")))
                {
                    mutexed.Modify(new Directives().Xpath("/xoc").Set(content));
                    Assert.Equal(content, mutexed.Value("/xoc/text()", ""));
                }
            });
        }
    }
}
