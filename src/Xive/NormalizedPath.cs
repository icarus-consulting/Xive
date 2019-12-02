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
using Yaapii.Atoms;
using Yaapii.Atoms.Func;
using Yaapii.Atoms.Scalar;

namespace Xive
{
    /// <summary>
    /// A path which uses forward slashes for windows/linux compatibility
    /// </summary>
    public sealed class NormalizedPath : IText
    {
        private readonly string input;
        private readonly IScalar<string> normalized;

        /// <summary>
        /// A path which uses forward slashes for windows/linux compatibility
        /// </summary>
        public NormalizedPath(string path)
        {
            this.input = path;
            this.normalized =
                new ScalarOf<string>(() =>
                    {
                        return input.Replace("\\", "/");
                    }
                );
        }

        public string AsString()
        {
            return this.normalized.Value();
        }

        public bool Equals(IText other)
        {
            return this.input.Equals(other.AsString());
        }
    }
}
