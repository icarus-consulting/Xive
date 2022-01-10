using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yaapii.Atoms.List;

namespace Xive.Props
{
    public sealed class CachedProps : IProps
    {
        private readonly IProps origin;
        private readonly ConcurrentDictionary<string, string[]> cache;

        public CachedProps(IProps origin, ConcurrentDictionary<string, string[]> cache)
        {
            this.origin = origin;
            this.cache = cache;
        }

        public IList<string> Names()
        {
            return new ListOf<string>(this.cache.Keys);
        }

        public IProps Refined(string prop, params string[] value)
        {
            if(value.Length == 0)
            {
                this.cache.TryRemove(prop, out string[] v);
            }
            else
            {
                this.cache.AddOrUpdate(prop, value, (key, v) => value);
            }
            return
                new CachedProps(
                    this.origin.Refined(prop, value),
                    this.cache
                );
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
            return
                new ListOf<string>(
                    this.cache.GetOrAdd(prop, key => new string[] { })
                );
        }
    }
}
