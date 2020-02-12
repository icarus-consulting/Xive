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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.List;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Props
{
    /// <summary>
    /// Props which are stored in memory.
    /// </summary>
    public sealed class RamProps : IProps
    {
        private readonly ConcurrentDictionary<string, string[]> props;

        /// <summary>
        /// Props which are stored in memory.
        /// </summary>
        public RamProps() : this(new ConcurrentDictionary<string, string[]>())
        { }

        /// <summary>
        /// Props which are stored in memory.
        /// </summary>
        public RamProps(ConcurrentDictionary<string, string[]> props)
        {
            this.props = props;
        }

        public IProps Refined(string prop, params string[] value)
        {
            return this.Refined(prop, new List<string>(value));
        }

        public IProps Refined(string prop, IEnumerable<string> values)
        {
            if (!values.GetEnumerator().MoveNext())
            {
                this.props[prop] = new string[0];
            }
            else
            {
                this.props[prop] = values.ToArray();
            }
            return this;
        }

        public string Value(string prop, string def = "")
        {
            IList<string> values = this.Values(prop);
            var result = def;
            if (values.Count > 1)
            {
                throw new InvalidOperationException($"There are multiple values for '{prop}', but you tried to access a single one.");
            }
            else if (values.Count == 1)
            {
                result = values[0];
            }
            return result;
        }

        public IList<string> Values(string prop)
        {
            string[] values = new string[0];
            if (!this.props.TryGetValue(prop, out values))
            {
                values = new string[0];
            }
            return new ListOf<string>(values);
        }

        public IList<string> Names()
        {
            return new ListOf<string>(this.props.Keys);
        }

        public void Save()
        { }
    }
}
