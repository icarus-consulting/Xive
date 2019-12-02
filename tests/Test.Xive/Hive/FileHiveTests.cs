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
using System.IO;
using Test.Yaapii.Xive;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Hive.Test
{
    public sealed class FileHiveTests
    {
        [Fact]
        public void DeliversHQ()
        {
            using (var dir = new TempDirectory())
            {                
                Assert.Equal(
                    $"product"+ Path.AltDirectorySeparatorChar + "hq",
                    new FileHive(
                        "product",
                        dir.Value().FullName
                    )
                    .HQ()
                    .Name()
                );
            }
        }

        [Fact]
        public void FindsComb()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive("product", dir.Value().FullName);
                new MutexCatalog(hive).Create("2CV");
                Assert.NotEmpty(
                    hive.Combs("@id='2CV'")
                );
            }
        }

        [Fact]
        public void ShiftsHQ()
        {
            using (var dir = new TempDirectory())
            {
                var hive = 
                    new CachedHive(
                        new FileHive("cockpit", dir.Value().FullName)
                    );
                new MutexCatalog(hive).Create("log");

                string a;
                string b;
                using (var cat = hive.HQ().Xocument("catalog.xml"))
                {
                    a = cat.Node().ToString();
                }

                using (var shifted = hive.Shifted("factory").HQ().Xocument("catalog.xml"))
                {
                    b = shifted.Node().ToString();
                }
            }
        }

        [Fact]
        public void PrependsScopeToCombName()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName);
                var shifted = hive.Shifted("prepend-this");
                new MutexCatalog(shifted).Create("an-entry");

                Assert.StartsWith("prepend-this",
                    new FirstOf<IHoneyComb>(
                        shifted.Combs("@id='an-entry'")
                    ).Value().Name()
                );
            }
        }

        [Fact]
        public void ShiftCreatesSubDir()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName);
                hive = hive.Shifted("product");
                new MutexCatalog(hive).Create("2CV");

                using (var cell =
                    new FirstOf<IHoneyComb>(
                        hive.Combs("@id='2CV'")
                    ).Value().Cell("data")
                )
                {
                    cell.Update(new InputOf("bytes over bytes here..."));
                    Assert.True(
                        Directory.Exists(Path.Combine(dir.Value().FullName, "product", "2CV"))
                    );
                }
            }
        }

        [Fact]
        public void ShiftCreatesHQ()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName);
                hive = hive.Shifted("product");
                new MutexCatalog(hive).Create("2CV");
                Assert.True(
                    Directory.Exists(Path.Combine(dir.Value().FullName, "product", "HQ"))
                );
            }
        }

        [Fact]
        public void ShiftsScope()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName);
                hive.Shifted("product");
                new MutexCatalog(hive).Create("2CV");

                hive = hive.Shifted("machine");
                new MutexCatalog(hive).Create("DrRobotic");
                Assert.NotEmpty(
                    hive.Combs("@id='DrRobotic'")
                );
            }
        }

        [Fact]
        public void CreatesHiveFolder()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive("product", dir.Value().FullName);
                new MutexCatalog(hive).Create("2CV");

                using (var cell = new FirstOf<IHoneyComb>(hive.Combs("@id='2CV'")).Value().Cell("Some-testing-item"))
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                    var productDir = Path.Combine(dir.Value().FullName, "product");
                    Assert.True(
                        Directory.Exists(productDir),
                        $"Directory '{productDir}' doesn't exist"
                    );
                }
            }
        }

        [Fact]
        public void CreatesCombFolder()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive("product", dir.Value().FullName);
                new MutexCatalog(hive).Create("2CV");

                using (var cell =
                    new FirstOf<IHoneyComb>(
                        hive.Combs("@id='2CV'")
                    ).Value().Cell("Some-testing-item")
                )
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                    var combDir = Path.Combine(dir.Value().FullName, "product", "2CV");
                    Assert.True(
                        Directory.Exists(combDir),
                        $"Directory '{combDir}' doesn't exist"
                    );
                }
            }
        }

        [Fact]
        public void RemembersCombCell()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive("product", dir.Value().FullName);
                new MutexCatalog(hive).Create("2CV");

                using (var cell =
                    new FirstOf<IHoneyComb>(
                        hive.Combs("@id='2CV'")
                    ).Value().Cell("Some-testing-item")
                )
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                }

                using (var cell =
                    new FirstOf<IHoneyComb>(
                        hive.Combs("@id='2CV'")
                    ).Value().Cell("Some-testing-item")
                )
                {
                    Assert.Equal(
                        "I am a very cool testdata string",
                        new TextOf(
                            cell.Content()
                        ).AsString()
                    );
                }
            }
        }

        [Fact]
        public void FailsOnEmptyDirectory()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Throws<ArgumentException>(() =>
                    new FileHive("product", "").Combs("@id='karre'")
                );
            }
        }

        [Fact]
        public void FailsOnMissingRootPath()
        {
            using (var dir = new TempDirectory())
            {
                Assert.Throws<ArgumentException>(() =>
                    new FileHive("product", "dir").Combs("@id='karre'")
                );
            }
        }

        [Fact]
        public void HasCorrectName()
        {
            Assert.Equal("the name", new FileHive("the name", "the root").Scope());
        }
    }
}
