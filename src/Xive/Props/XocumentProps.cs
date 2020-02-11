﻿//MIT License

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

using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IText xpath;
        private readonly IXocument catalog;
        private readonly Solid<IProps> memoryProps;

        /// <summary>
        /// Props which are read into memory from internal xml document _catalog.xml in the given comb.
        /// Props are read from memory.
        /// Props are updated into the comb.
        /// </summary>
        public XocumentProps(IXocument catalog, string id)
        {
            this.xpath = new TextOf(() => $"/catalog/*[@id='{id}']");
            this.catalog = catalog;
            this.memoryProps = new Solid<IProps>(() =>
            {
                Debug.WriteLine("Read props for " + id);
                IDictionary<string, IList<string>> props = new Dictionary<string, IList<string>>();
                bool exists = false;
                foreach (var entity in catalog.Nodes(this.xpath.AsString()))
                {
                    exists = true;
                    foreach (var prop in entity.Nodes("./props/*"))
                    {
                        var name = prop.Values("./name/text()");
                        if (name.Count == 1)
                        {
                            var values = prop.Values("./values/item/text()");
                            props.Add(name[0], values);
                        }
                    }
                }
                if (!exists)
                {
                    catalog.Modify(new Directives().Xpath("/catalog").Add("item").Attr("id", id));
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
                new Directives()
                    .Xpath($"{this.xpath.AsString()}/props")
                    .Remove()
                    .Xpath(this.xpath.AsString());

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
