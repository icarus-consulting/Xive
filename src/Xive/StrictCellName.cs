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
    /// A cell name which does reject invalid chars.
    /// </summary>
    public sealed class StrictCellName : IText
    {
        private readonly IScalar<string> validated;

        /// <summary>
        /// A cell name which does reject invalid chars.
        /// </summary>
        public StrictCellName(string name)
        {
            this.validated =
                new ScalarOf<string>(() =>
                    {
                        name = new Normalized(name).AsString();
                        if (name.Contains(" ") || name.Contains("\r") || name.Contains("\n"))
                        {
                            throw new ArgumentException($"Can't use '{name}' as name because it contains whitespaces");
                        }
                        if (name.Contains("&") || name.Contains("<") || name.Contains(">"))
                        {
                            throw new ArgumentException($"Can't use '{ name }' as name because it contains illegal chars (&,< or >)");
                        }
                        return name;
                    }
                );
        }

        public string AsString()
        {
            return this.validated.Value();
        }

        public bool Equals(IText other)
        {
            return this.validated.Value().Equals(other.AsString());
        }
    }
}
