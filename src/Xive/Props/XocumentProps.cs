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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;
using Yaapii.Xambly;

namespace Xive.Props
{
    /// <summary>
    /// Props which are read into memory from internal xml document _catalog.xml in the given comb.
    /// Props are read from memory.
    /// Props are updated into the comb.
    /// </summary>
    public sealed class XocumentProps : IProps
    {
        private readonly IXocument catalog;
        private readonly Sticky<IProps> memoryProps;
        private readonly string id;
        private readonly string scope;

        /// <summary>
        /// Props which are read into memory from internal xml document _catalog.xml in the given comb.
        /// Props are read from memory.
        /// Props are updated into the comb.
        /// </summary>
        public XocumentProps(IXocument catalog, string scope, string id)
        {
            this.id = id;
            this.scope = scope;
            this.catalog = catalog;
            this.memoryProps = new Sticky<IProps>(() =>
            {
                ConcurrentDictionary<string, string[]> props = new ConcurrentDictionary<string, string[]>();
                foreach (var entity in catalog.Nodes($"/catalog/{scope}[@id='{id}']"))
                {
                    foreach (var prop in entity.Nodes("./props/prop"))
                    {
                        var name = prop.Values("./name/text()");
                        if (name.Count == 1)
                        {
                            var values = prop.Values("./values/item/text()").ToArray();
                            props.AddOrUpdate(
                                name[0],
                                values,
                                (key, current) => values
                            );
                        }
                    }
                }
                return new RamProps(props);
            });
        }

        public IProps Refined(string prop, params string[] value)
        {
            this.memoryProps.Value().Refined(prop, value);
            Save();
            return this;
        }

        public IProps Refined(params IPropsInput[] props)
        {
            this.memoryProps.Value().Refined(props);
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
            var patch =
                new Directives(
                    new Directives()
                        .Xpath("/catalog")
                        .Append(new AddIfAttributeDirective(this.scope.ToLower(), "id", this.id.ToLower()))
                    )
                    .Xpath($"/catalog/{this.scope.ToLower()}[@id='{this.id.ToLower()}']/props")
                    .Remove()
                    .Xpath($"/catalog/{this.scope.ToLower()}[@id='{this.id.ToLower()}']");

            foreach (var prop in this.memoryProps.Value().Names())
            {
                patch.Add("props")
                    .Add("prop")
                    .Add("name")
                    .Set(prop)
                    .Up()
                    .Add("values");
                foreach (var value in this.memoryProps.Value().Values(prop))
                {
                    patch.Add("item").Set(value).Up();
                }
                patch.Up();
            }

            using (var xoc = catalog)
            {
                xoc.Modify(patch);
            }
        }
    }
}
