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
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

namespace Xive.Mnemonic.Test
{
    public sealed class RamMnemonicTests
    {
        [Fact]
        public void MemorizesProps()
        {
            var mem = new RamMnemonic();
            new TextIndex(
                "beverage",
                mem
            ).Add("fritz-kola");

            mem.Props("beverage", "fritz-kola").Refined("light", "yes please");

            Assert.Equal(
                "yes please",
                mem.Props("beverage", "fritz-kola").Value("light")
            );

        }

        [Fact]
        public void MemorizesXml()
        {

            var mem = new RamMnemonic();
            new MemorizedComb("beverage", mem)
                .Xocument("coke.xml")
                .Modify(new Directives().Xpath("/coke").Add("light").Set("yes please"));


            Assert.Equal(
                "yes please",
                new MemorizedComb("beverage", mem)
                    .Xocument("coke.xml")
                    .Value("/coke/light/text()", "")
            );

        }

        [Fact]
        public void MemorizesBinary()
        {
            var mem = new RamMnemonic();
            new MemorizedComb("beverage", mem)
                .Cell("pepsi")
                .Update(new InputOf("Empty"));

            Assert.Equal(
                "Empty",
                new TextOf(
                    new InputOf(
                        new MemorizedComb("beverage", mem)
                            .Cell("pepsi")
                            .Content()
                    )
                ).AsString()
            );
        }
    }
}