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
using System.Text;
using Xunit;

namespace Xive.Props.Test
{
    public sealed class DecodedPropTests
    {
        [Fact]
        public void ReplacesColon()
        {
            Assert.Equal(
                ":",
                new DecodedProp("&#42").Value()
            );
        }

        [Fact]
        public void ReplacesComma()
        {
            Assert.Equal(
                ",",
                new DecodedProp("&#43").Value()
            );
        }

        [Fact]
        public void ReplacesMultiple()
        {
            Assert.Equal(
                "this,is,a,long,text,with:colons:and:commas",
                new DecodedProp("this&#43is&#43a&#43long&#43text&#43with&#42colons&#42and&#42commas").Value()
            );
        }
    }
}
