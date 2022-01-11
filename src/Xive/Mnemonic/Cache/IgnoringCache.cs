//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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
using System.Text.RegularExpressions;
using Yaapii.Atoms.Func;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic.Cache
{
    /// <summary>
    /// A cache which ignores the given items.
    /// </summary>
    public sealed class IgnoringCache<TData> : ICache<TData>
    {
        private readonly ScalarOf<string[]> patterns;
        private readonly BiFuncOf<string, TData, bool> skip;
        private readonly ICache<TData> origin;

        /// <summary>
        /// A cache which ignores the given items.
        /// </summary>
        public IgnoringCache(ICache<TData> origin, IEnumerable<string> ignored)
        {
            this.patterns =
                new ScalarOf<string[]>(() =>
                {
                    var items = new ListOf<string>(ignored);
                    var compiled = new string[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        compiled[i] = Regex.Escape(items[i].ToLower()).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                    }
                    return compiled;
                });

            this.origin = origin;
        }

        public TData Content(string name, Func<TData> ifAbsent)
        {
            TData result;
            if (!IsIgnored(name))
            {
                result = this.origin.Content(name, ifAbsent);
            }
            else
            {
                result = ifAbsent();
            }
            return result;
        }

        public void Remove(string name)
        {
            if (!IsIgnored(name))
            {
                this.origin.Remove(name);
            }
        }

        public void Update(string name, Func<TData> ifAbsent, Func<TData> ifExists)
        {
            if (!IsIgnored(name))
            {
                this.origin.Update(name, ifAbsent, ifExists);
            }
        }

        private bool IsIgnored(string name)
        {
            var result = false;
            var patterns = this.patterns.Value();
            for (var i = 0; i < this.patterns.Value().Length; i++)
            {
                if (Regex.IsMatch(name, patterns[i]))
                {
                    result = true;
                    break;
                }
            }
            return result;

        }
    }
}
