using System;
using System.Collections.Generic;
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
                IDictionary<string, IList<string>> props = new Dictionary<string, IList<string>>();
                using (catalog)
                {
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
