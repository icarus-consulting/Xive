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
using System.IO;
using System.Text.RegularExpressions;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive
{
    /// <summary>
    /// A coordinate which disallows illegal chars in comb names. 
    /// A path to a cell consists of one to multiple coordinates and a cellname.
    /// </summary>
    public sealed class StrictCoordinate : IText
    {
        private readonly Solid<string> validated;

        /// <summary>
        /// A coordinate which disallows illegal chars in comb names. 
        /// A path to a cell consists of one to multiple coordinates and a cellname.
        /// </summary>
        /// <param name="text"></param>
        public StrictCoordinate(string text)
        {
            this.validated = new Solid<string>(() =>
            {
                string regexSearch = new string(Path.GetInvalidPathChars());
                Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

                if (r.Matches(text).Count > 0)
                {
                    throw new ArgumentException($"'{text}' is not a valid coordinate, it contains illegal characters. Illegal characters are: $',\\r,\\n,{String.Join(",", Path.GetInvalidPathChars())}'");
                }
                return text;
            });
        }

        public string AsString()
        {
            return this.validated.Value();
        }

        public bool Equals(IText other)
        {
            return other.AsString().Equals(this.AsString());
        }
    }
}
