////MIT License

////Copyright (c) 2022 ICARUS Consulting GmbH

////Permission is hereby granted, free of charge, to any person obtaining a copy
////of this software and associated documentation files (the "Software"), to deal
////in the Software without restriction, including without limitation the rights
////to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
////copies of the Software, and to permit persons to whom the Software is
////furnished to do so, subject to the following conditions:

////The above copyright notice and this permission notice shall be included in all
////copies or substantial portions of the Software.

////THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
////IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
////FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
////AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
////LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
////OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
////SOFTWARE.

//using Xive.Mnemonic;
//using Xive.Xocument;
//using Xunit;
//using Yaapii.Xambly;

//#pragma warning disable MaxPublicMethodCount // a public methods count maximum

//namespace Xive.Hive.Test
//{
//    public sealed class XiveIndexTests
//    {
//        [Fact]
//        public void AddsItems()
//        {
//            var idx = new XiveIndex("test", new RamMemories());
//            idx.Add("123");
//            Assert.True(idx.Has("123"));
//        }

//        [Fact]
//        public void RemovesItems()
//        {
//            var idx = new XiveIndex("test", new RamMemories());
//            idx.Add("123");
//            idx.Remove("123");
//            Assert.False(idx.Has("123"));
//        }

//        [Fact]
//        public void FiltersItems()
//        {
//            var mem = new RamMemories();
//            var idx = new XiveIndex("test", mem);
//            idx.Add("123");
//            idx.Add("456");
//            mem.Props("test", "123").Refined("works", "true");
//            mem.Props("test", "456").Refined("works", "false");

//            Assert.Equal(
//                "test/456", idx.List(new IndexFilterOf(props => props.Value("works", "") == "false"))[0].Name()
//            );
//        }

//        [Fact]
//        public void CachesItems()
//        {
//            var mem = new RamMemories();
//            var idx = new XiveIndex("test", mem);
//            idx.Add("123");

//            new MemorizedXocument(
//                $"test/hq/catalog.xml", 
//                mem
//            ).Modify(
//                new Directives().Xpath("/catalog/*").Remove()
//            );
//            Assert.True(idx.Has("123"));
//        }

//        [Fact]
//        public void ReloadsCacheAfterAdd()
//        {
//            var mem = new RamMemories();
//            var idx = new XiveIndex("test", mem);
//            idx.Add("123");

//            new MemorizedXocument(
//                $"test/hq/catalog.xml",
//                mem
//            ).Modify(
//                new Directives().Xpath("/catalog/*").Remove()
//            );

//            idx.Add("456"); //trigger reloading from file by updating index

//            Assert.False(idx.Has("123"));
//        }

//        [Fact]
//        public void ReloadsCacheAfterRemove()
//        {
//            var mem = new RamMemories();
//            var idx = new XiveIndex("test", mem);
//            idx.Add("123");

//            new MemorizedXocument(
//                $"test/hq/catalog.xml",
//                mem
//            ).Modify(
//                new Directives().Xpath("/catalog/*").Remove()
//            );

//            idx.Add("456");
//            idx.Remove("456"); //trigger reloading from file by updating index

//            Assert.False(idx.Has("123"));
//        }
//    }
//}
