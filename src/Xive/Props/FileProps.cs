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
    public sealed class SimpleProps : IProps
    {
        private readonly IText xpath;
        private readonly IHoneyComb hq;
        private readonly Solid<IProps> memoryProps;

        /// <summary>
        /// Props which are read into memory from internal xml document _catalog.xml in the given comb.
        /// Props are read from memory.
        /// Props are updated into the comb.
        /// </summary>
        public SimpleProps(IHoneyComb comb, string id)
        {
            this.xpath = new TextOf(() => $"/catalog/{comb.Name()}[@id='{id}']");
            this.hq = comb;
            this.memoryProps = new Solid<IProps>(() =>
            {
                IDictionary<string, IList<string>> props = new Dictionary<string, IList<string>>();
                using (var xoc = comb.Xocument("catalog.xml"))
                {
                    foreach (var entity in xoc.Nodes(this.xpath.AsString()))
                    {
                        var ids = entity.Values("@id");
                        if (ids.Count == 1)
                        {
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
                    .Remove();

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

            using (var xoc = this.hq.Xocument("catalog.xml"))
            {
                xoc.Modify(patch);
            }
        }
    }
}
