//using System.Collections.Generic;
//using Xunit;
//using Xive.Comb;
//using Xive.Hive;

//namespace Xive.Farm.Test
//{
//    public sealed class SimpleFarmTests
//    {
//        [Fact]
//        public void DeliversHive()
//        {
//            var mem = new Dictionary<string, byte[]>();

//            Assert.Equal(
//                "my-hive/HQ",
//                new SimpleFarm(hive => 
//                    new SimpleHive(hive, comb => 
//                        new RamComb(comb, mem)
//                    )
//                ).Hive("my-hive").HQ().Name()
//            );
//        }
//    }
//}
