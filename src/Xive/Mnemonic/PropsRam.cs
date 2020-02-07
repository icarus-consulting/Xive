using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xive.Hive;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Cache
{
    /// <summary>
    /// Memory for XML props.
    /// </summary>
    internal sealed class PropsRam : IMemory<IProps>
    {
        private readonly IScalar<ConcurrentDictionary<string, IProps>> mem;

        /// <summary>
        /// Memory for props.
        /// </summary>
        internal PropsRam() : this(new ConcurrentDictionary<string, IProps>())
        { }

        /// <summary>
        /// Memory for props.
        /// </summary>
        internal PropsRam(string scope, string id, IProps props) : this(new Solid<ConcurrentDictionary<string, IProps>>(() =>
            {
                var mem = new ConcurrentDictionary<string, IProps>();
                mem.AddOrUpdate(scope, props, (n, p) => props);
                return mem;
            })
        )
        { }

        /// <summary>
        /// Memory for props.
        /// </summary>
        internal PropsRam(ConcurrentDictionary<string, IProps> mem) : this(new ScalarOf<ConcurrentDictionary<string, IProps>>(mem))
        { }

        /// <summary>
        /// Memory for props.
        /// </summary>
        internal PropsRam(IScalar<ConcurrentDictionary<string, IProps>> mem)
        {
            this.mem = mem;
        }

        public bool Knows(string name)
        {
            return this.mem.Value().ContainsKey(name);
        }

        public IEnumerable<string> Knowledge()
        {
            return this.mem.Value().Keys;
        }

        public IProps Content(string name, Func<IProps> ifAbsent)
        {
            name = new Normalized(name).AsString();
            return this.mem.Value().GetOrAdd(name, (key) => ifAbsent());
        }

        public void Update(string name, IProps content)
        {
            name = new Normalized(name).AsString();
            this.mem.Value().AddOrUpdate(name, content, (currentName, currentContent) => content);
        }
    }
}
