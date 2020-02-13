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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xive.Mnemonic;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;
using Yaapii.Xml;

namespace Xive.Props
{
    /// <summary>
    /// Props which are read into memory from internal xml document _catalog.xml in the given comb.
    /// Props are read from memory.
    /// Props are updated into the comb.
    /// </summary>
    public sealed class SandboxProps : IProps
    {
        private readonly IMnemonic mem;
        private readonly Sticky<IProps> memoryProps;
        private readonly string id;
        private readonly string scope;

        /// <summary>
        /// Props which are read into memory from internal xml document _catalog.xml in the given comb.
        /// Props are read from memory.
        /// Props are updated into the comb.
        /// </summary>
        public SandboxProps(IMnemonic mem, string scope, string id)
        {
            this.id = id;
            this.scope = scope;
            this.mem = mem;
            this.memoryProps = new Sticky<IProps>(() =>
            {
                var stringProps =
                    new TextOf(
                        this.mem
                            .Data()
                            .Content(
                                new Normalized($"{scope}/{id}/props.cat").AsString(),
                                () => new byte[0]
                            )
                        ).AsString();

                var cachedProps = new RamProps();
                Parallel.ForEach(stringProps.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries), (stringProp) =>
                {
                    var parts = stringProp.Split(':');
                    if (parts.Length != 2)
                    {
                        throw new ApplicationException($"A property of {scope}/{id} has an invalid format: {stringProp}");
                    }
                    var name = parts[0].Trim();
                    var values = parts[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    cachedProps.Refined(name, values);
                });
                return cachedProps;
            });
        }

        public IProps Refined(string prop, params string[] value)
        {
            this.memoryProps.Value().Refined(prop, value);
            Save();
            return this;
        }

        public string Value(string prop, string def = "")
        {
            return this.memoryProps.Value().Value(prop, def);
        }

        public IList<string> Values(string prop)
        {
            return this.memoryProps.Value().Values(prop);
        }

        public IList<string> Names()
        {
            return this.memoryProps.Value().Names();
        }

        private void Save()
        {
            string serialized = string.Empty;
            foreach (var prop in this.memoryProps.Value().Names())
            {
                serialized += $"{prop}:{string.Join(",", memoryProps.Value().Values(prop))}\r";
            }
            var data = new BytesOf(serialized).AsBytes();

            this.mem
                .Data()
                .Update(
                    new Normalized($"{scope}/{id}/props.cat").AsString(),
                    data
                );
        }

        private XDocument Bootstrapped()
        {
            return new XDocument(new XElement("props", new XAttribute("id", id)));
        }
    }
}
