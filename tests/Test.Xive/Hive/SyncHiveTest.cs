using System;
using System.Threading.Tasks;
using Xunit;
using Yaapii.Atoms.IO;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public class SyncHiveTest
    {
        [Fact]
        public void CreatesCatalogInParallel()
        {
            var hive = new SyncHive(new RamHive("testRamHive"));

            Parallel.For(0, Environment.ProcessorCount << 4, (i) =>
            {
                var catalog = new Catalog(hive);
                catalog.Create("123");
            });
        }

        [Fact]
        public void DeliversCombsInParallel()
        {
            var hive = new SyncHive(new RamHive("product"));
            new Catalog(hive).Create("2CV");

            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.Combs("@id='2CV'");
            });
            
        }

        [Fact]
        public void DeliversHQInParallel()
        {
            var hive =
                new SyncHive(
                    new RamHive("person")
                );
            Parallel.For(0, Environment.ProcessorCount << 4, i =>
            {
                hive.HQ();
            });
        }

        [Fact]
        public void DeliversNameInParallel()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new SyncHive(new FileHive("product", dir.Value().FullName));
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    hive.Name();
                });
            }
        }

        [Fact]
        public void WorksWithFileHive()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new SyncHive(new FileHive("product", dir.Value().FullName));
                var catalog = new Catalog(hive);
                catalog.Create("2CV");
                Parallel.For(0, Environment.ProcessorCount << 4, i =>
                {
                    hive.Combs("@id='2CV'");
                    hive.Name();
                    hive.HQ();
                });
            }
        }
    }
}

