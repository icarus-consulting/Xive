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
                    $"product/hq",
                    new FileHive(dir.Value().FullName, "product")
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
                var hive = new FileHive(dir.Value().FullName, "product");
                hive.Catalog().Add("2CV");
                Assert.NotEmpty(
                    hive.Catalog().List()
                );
            }
        }

        [Fact]
        public void ShiftsHQ()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "cockpit");
                hive.Catalog().Add("log");

                var shifted = hive.Shifted("factory");
                shifted.Catalog().Add("booyaa");

                var shiftedAgain = shifted.Shifted("cockpit");

                Assert.Contains("log", shiftedAgain.Catalog().List()[0].Name());
            }
        }

        [Fact]
        public void PrependsScopeToCombName()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName, "cockpit");
                var shifted = hive.Shifted("prepend-this");
                shifted.Catalog().Add("an-entry");

                Assert.StartsWith("prepend-this",
                    shifted.Comb("an-entry").Name()
                );
            }
        }

        [Fact]
        public void ShiftCreatesSubDir()
        {
            using (var dir = new TempDirectory())
            {
                IHive hive = new FileHive(dir.Value().FullName, "cars");
                hive = hive.Shifted("product");
                hive.Catalog().Add("2CV");

                using (var cell =
                    hive.Comb("2CV").Cell("data")
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
                new FileHive(dir.Value().FullName, "product").Catalog().Add("2CV");
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
                IHive hive = new FileHive(dir.Value().FullName, "product");
                hive.Catalog().Add("2CV");
                hive = hive.Shifted("machine");
                hive.Catalog().Add("DrRobotic");
                Assert.Equal(
                    1,
                    hive.Catalog().List().Count
                );
            }
        }

        [Fact]
        public void CreatesHiveFolder()
        {
            using (var dir = new TempDirectory())
            {
                var hive = new FileHive(dir.Value().FullName, "product");
                hive.Catalog().Add("2CV");

                using (var cell = hive.Comb("2CV").Cell("Some-testing-item"))
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
                var hive = new FileHive(dir.Value().FullName, "product");
                using (var cell =
                    hive.Comb("2CV", true).Cell("Some-testing-item")
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
                var hive = new FileHive(dir.Value().FullName, "product");
                using (var cell = hive.Comb("2CV").Cell("Some-testing-item")
                )
                {
                    cell.Update(new InputOf("I am a very cool testdata string"));
                }

                using (var cell = hive.Comb("2CV").Cell("Some-testing-item")
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
        public void DeliversScope()
        {
            Assert.Equal("the name", new FileHive("the root", "the name").Scope());
        }
    }
}
