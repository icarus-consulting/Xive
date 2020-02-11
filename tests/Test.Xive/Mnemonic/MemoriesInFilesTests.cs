//MIT License

//Copyright (c) 2020 ICARUS Consulting GmbH

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

using Xive.Comb;
using Xive.Hive;
using Xive.Mnemonic;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

namespace Xive.Mnemonic.Test
{
    public sealed class MemoriesInFilesTests
    {
        [Fact]
        public void MemorizesProps()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new FileMemories(temp.Value().FullName);
                new XiveIndex(
                    "beverage", 
                    mem
                ).Add("fritz-kola");

                mem.Props("beverage", "fritz-kola").Refined("light", "yes please");

                mem = new FileMemories(temp.Value().FullName);

                Assert.Equal(
                    "yes please",
                    mem.Props("beverage", "fritz-kola").Value("light")
                );
            }
        }

        [Fact]
        public void MemorizesXml()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new FileMemories(temp.Value().FullName);
                using (var xoc = new MemorizedComb("beverage", mem).Xocument("coke.xml"))
                {
                    xoc.Modify(new Directives().Xpath("/coke").Add("light").Set("yes please"));
                }

                mem = new FileMemories(temp.Value().FullName);
                using (var xoc = new MemorizedComb("beverage", mem).Xocument("coke.xml"))
                {
                    Assert.Equal(
                        "yes please",
                        xoc.Value("/coke/light/text()", "")
                    );
                }
            }
        }

        [Fact]
        public void MemorizesBinary()
        {
            using (var temp = new TempDirectory())
            {
                var mem = new FileMemories(temp.Value().FullName);
                using (var pepsi = new MemorizedComb("beverage", mem).Cell("pepsi"))
                {
                    pepsi.Update(new InputOf("Empty"));
                }

                mem = new FileMemories(temp.Value().FullName);
                using (var pepsi = new MemorizedComb("beverage", mem).Cell("pepsi"))
                {
                    Assert.Equal(
                        "Empty",
                        new TextOf(
                            new InputOf(pepsi.Content())
                        ).AsString()
                    );
                }
            }
        }
    }
}
