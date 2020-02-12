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
using Xunit;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Props.Test
{
    public sealed class RamPropsTests
    {
        [Fact]
        public void DeliversDefault()
        {
            Assert.Equal(
                "I MUST INSIST: I EXIST!",
                new RamProps()
                    .Value("not-existing", "I MUST INSIST: I EXIST!")
            );
        }

        [Fact]
        public void DeliversSingleValue()
        {
            Assert.Equal(
                "good boy!",
                new RamProps()
                    .Refined("behaviour", "good boy!")
                    .Value("behaviour")
            );
        }

        [Fact]
        public void OverwritesValue()
        {
            Assert.Equal(
                "average boy.",
                new RamProps()
                    .Refined("behaviour", "good boy!")
                    .Refined("behaviour", "average boy.")
                    .Value("behaviour")
            );
        }

        [Fact]
        public void DeliversMultipleValues()
        {
            Assert.Equal(
                "good boy, bad boy",
                new Yaapii.Atoms.Text.Joined(", ",
                    new RamProps()
                        .Refined("name", "Mr Jekyll/Mr. Hide")
                        .Refined("behaviour", "good boy", "bad boy")
                        .Values("behaviour")
                ).AsString()
            );
        }

        //[Fact]
        //public void AppliesPropsInput()
        //{
        //    Assert.Equal(
        //        "",
        //        new RamProps()
        //            .Refined("behaviour", "nasty boy")
        //            .Refined(
        //                new FkPropsInput(props => { props.Remove("behaviour"); return props; })
        //            )
        //            .Value("behaviour")
        //    );
        //}

        [Fact]
        public void RejectsSingleWhenMultipleExist()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new RamProps()
                    .Refined("name", "Mr Jekyll/Mr. Hide")
                    .Refined("behaviour", "good boy", "bad boy")
                    .Value("behaviour")
            );
        }
    }
}
