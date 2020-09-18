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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xive.Cache;
using Xive.Mnemonic.Content;
using Xunit;
using Yaapii.Atoms.IO;

namespace Xive.Props.Test
{
    public sealed class SandboxPropsTests
    {
        [Fact]
        public void ReturnsNames()
        {
            var props = new SandboxProps(new SimpleMnemonic(new RamContents()), "scope", "id");
            props.Refined("the prop", "the value");
            Assert.Equal(
                new List<string>() { "the prop" },
                props.Names()
            );
        }

        [Fact]
        public void ReturnsValue()
        {
            var props = new SandboxProps(new RamContents(), "scope", "id");
            props.Refined("the prop", "the value");
            Assert.Equal(
                "the value",
                props.Value("the prop")
            );
        }

        [Fact]
        public void IsThreadSafe()
        {
            using (var dir = new TempDirectory())
            {
                var contents =
                    new CachedContents(
                        new FileContents(dir.Value().FullName)
                    );
                var props = new SandboxProps(contents, "scope", "id");
                props.Refined("the prop", "the value");

                Parallel.For(0, 1000, i =>
                {
                    Thread.Sleep(new Random().Next(0,100));
                    props.Value(i.ToString());
                    Assert.Equal(
                        "the value",
                        new SandboxProps(contents, "scope", "id").Value("the prop")
                    );
                });
            }
            
        }

        [Fact]
        public void ReturnsValues()
        {
            var props = new SandboxProps(new RamContents(), "scope", "id");
            props.Refined("the prop", "the value", "another value");
            Assert.Equal(
                new List<string>() { "the value", "another value" },
                props.Values("the prop")
            );
        }

        [Fact]
        public void AcceptsCommaInValues()
        {
            var props = new SandboxProps(new RamContents(), "scope", "id");
            props.Refined("the,:prop", "the,:value", "another,:value");
            Assert.Equal(
                new List<string>() { "the,:value", "another,:value" },
                props.Values("the,:prop")
            );
        }

        [Fact]
        public void AcceptsCommaInValue()
        {
            var props = new SandboxProps(new RamContents(), "scope", "id");
            props.Refined("the,:prop", "the,:value");
            Assert.Equal(
                "the,:value",
                props.Value("the,:prop")
            );
        }
    }
}
