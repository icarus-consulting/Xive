//MIT License

//Copyright (c) 2019 ICARUS Consulting GmbH

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
using System.Collections.Generic;
using System.IO;
using Xive.Comb;
using Yaapii.Atoms.Enumerable;

namespace Xive.Hive
{
    /// <summary>
    /// A hive that lives in memory.
    /// </summary>
    public sealed class RamHive : IHive
    {
        private readonly string scope;
        private readonly Func<IHoneyComb, IHoneyComb> wrap;
        private readonly IDictionary<string, MemoryStream> memory;

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, this hive's contents will live only as long as this instance lives.
        /// </summary>
        /// <param name="scope">Scope name of the hive</param>
        public RamHive() : this("X")
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, this hive's contents will live only as long as this instance
        /// lives.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="catalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        public RamHive(string name) : this(
            name,
            comb => comb,
            new Dictionary<string, MemoryStream>()
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, you can construct the hive multiple times with the same name and memory
        /// and its combs will always have the same contents.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="memory">An external memory for the hive</param>
        /// <param name="combWrap">Wrap the comb if needed.</param>
        public RamHive(Func<IHoneyComb, IHoneyComb> combWrap, IDictionary<string, MemoryStream> memory) : this(
            "X",
            combWrap,
            memory
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, this hive's contents will live only as long as this instance lives.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="combWrap">Wrap the comb if needed.</param>
        public RamHive(Func<IHoneyComb, IHoneyComb> combWrap) : this(
            "X",
            combWrap,
            new Dictionary<string, MemoryStream>()
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, this hive's contents will live only as long as this instance lives.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="combWrap">Wrap the comb if needed.</param>
        public RamHive(string name, Func<IHoneyComb, IHoneyComb> combWrap) : this(
            name,
            combWrap,
            new Dictionary<string, MemoryStream>()
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, you can construct the hive multiple times with the same name and memory
        /// and its combs will always have the same contents.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="catalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        public RamHive(IDictionary<string, MemoryStream> memory) : this(
            "X",
            comb => comb,
            memory
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, you can construct the hive multiple times with the same name and memory
        /// and its combs will always have the same contents.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="catalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        public RamHive(string name, IDictionary<string, MemoryStream> memory) : this(
            name,
            comb => comb,
            memory
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, you can construct the hive multiple times with the same name and its combs will
        /// always have the same contents.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="catalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        /// <param name="memory">An external memory for the hive</param>
        public RamHive(string name, Func<IHoneyComb, IHoneyComb> combWrap, IDictionary<string, MemoryStream> memory)
        {
            this.scope = name;
            this.wrap = combWrap;
            this.memory = memory;
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return Combs(xpath, new MutexCatalog(this.scope, HQ()));
        }

        public IEnumerable<IHoneyComb> Combs(string xpath, ICatalog catalog)
        {
            return
                new Mapped<string, IHoneyComb>(
                    name => this.wrap(Comb(name)),
                    catalog.List(xpath)
                );
        }

        public IHoneyComb HQ()
        {
            return this.wrap(Comb("HQ"));
        }

        public IHive Shifted(string scope)
        {
            return new RamHive(scope, this.wrap, this.memory);
        }

        public string Scope()
        {
            return this.scope;
        }

        private IHoneyComb Comb(string name)
        {
            return new RamComb($"{this.scope}{Path.AltDirectorySeparatorChar}{name}", this.memory);
        }
    }
}
