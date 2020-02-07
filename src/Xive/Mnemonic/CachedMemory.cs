using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Xive.Hive;

namespace Xive.Mnemonic
{
    public sealed class CachedMemory<T> : IMemory<T>
    {
        private readonly ConcurrentDictionary<string, T> mem;
        private readonly List<string> knowledge;
        private readonly IMemory<T> origin;

        public CachedMemory(IMemory<T> origin)
        {
            this.mem = new ConcurrentDictionary<string, T>();
            this.origin = origin;
            this.knowledge = new List<string>();
        }

        public T Content(string name, Func<T> ifAbsent)
        {
            return
                this.mem.GetOrAdd(
                    name,
                    (key) =>
                    {
                        var result = this.origin.Content(name, ifAbsent);
                        this.knowledge.Clear();
                        this.knowledge.AddRange(this.origin.Knowledge());
                        return result;
                    });
        }

        public IEnumerable<string> Knowledge()
        {
            return this.knowledge;
        }

        public bool Knows(string name)
        {
            return this.mem.ContainsKey(name);
        }

        public void Update(string name, T content)
        {
            this.mem.AddOrUpdate(
                name, 
                key => 
                {
                    this.origin.Update(name, content);
                    this.knowledge.Clear();
                    this.knowledge.AddRange(this.origin.Knowledge());
                    return content;
                }, 
                (key, current) => {
                    this.origin.Update(name, content);
                    this.knowledge.Clear();
                    this.knowledge.AddRange(this.origin.Knowledge());
                    return content;
                });
        }
    }
}
