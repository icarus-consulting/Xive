using System;
using System.Collections.Generic;
using Xive.Hive;
using Yaapii.Atoms.Enumerable;

namespace Xive.Cache
{
    public sealed class LayeredMemory<T> : IMemory<T>
    {
        private readonly string layer;
        private readonly IMemory<T> origin;

        public LayeredMemory(string layer, IMemory<T> origin)
        {
            this.layer = layer;
            this.origin = origin;
        }

        public T Content(string name, Func<T> ifAbsent)
        {
            return this.origin.Content($"{this.layer}/{name}", ifAbsent);
        }

        public bool Knows(string name)
        {
            return this.origin.Knows($"{this.layer}/{name}");
        }

        public IEnumerable<string> Knowledge()
        {
            return 
                new Mapped<string, string>(
                    name => $"{this.layer}/{name}",
                    this.origin.Knowledge()
                );
        }

        public void Update(string name, T content)
        {
            this.origin.Update($"{this.layer}/{name}", content);
        }
    }
}
