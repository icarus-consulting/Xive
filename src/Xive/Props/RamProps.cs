using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.List;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive
{
    public sealed class RamProps : IProps
    {
        private readonly ConcurrentDictionary<string, IList<string>> props;

        public RamProps() : this(new ConcurrentDictionary<string, IList<string>>())
        { }

        public RamProps(IDictionary<string, IList<string>> props)
        {
            this.props = new ConcurrentDictionary<string, IList<string>>(props);
        }

        public IProps Refined(string prop, params string[] value)
        {
            return this.Refined(prop, new EnumerableOf<string>(value));
        }

        public IProps Refined(string prop, IEnumerable<string> values)
        {
            if (!values.GetEnumerator().MoveNext())
            {
                this.props[prop] = new List<string>();
            }
            else
            {
                this.props[prop] = new StickyList<string>(new List<string>(values));
            }
            return this;
        }

        public IProps Refined(params IPropsInput[] props)
        {
            foreach (var prop in props)
            {
                prop.Apply(this.props);
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
            IList<string> values = new List<string>();
            if (!this.props.TryGetValue(prop, out values))
            {
                values = new ListOf<string>();
            }
            return new ListOf<string>(values);
        }

        public IList<string> Names()
        {
            return new ListOf<string>(this.props.Keys);
        }
    }
}
