﻿//MIT License

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
        private readonly string name;
        private readonly Func<IHoneyComb, IHoneyComb> comb;
        private readonly Func<string, IHoneyComb, ICatalog> catalog;
        private readonly IDictionary<string, byte[]> memory;

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, this hive's contents will live only as long as this instance lives.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        public RamHive(string name) : this(
            name,
            (hiveName, comb) => new Catalog(hiveName, comb)
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
            (hiveName, cmb) => new Catalog(hiveName, cmb),
            new Dictionary<string, byte[]>()
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
        public RamHive(string name, Func<IHoneyComb, IHoneyComb> combWrap, IDictionary<string, byte[]> memory) : this(
            name,
            combWrap,
            (hiveName, comb) => new Catalog(hiveName, comb),
            memory
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, you can construct the hive multiple times with the same name and memory
        /// and its combs will always have the same contents.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="memory">An external memory for the hive</param>
        public RamHive(string name, IDictionary<string, byte[]> memory) : this(
            name,
            comb => comb,
            (hiveName, comb) => new Catalog(hiveName, comb),
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
        public RamHive(string name, Func<string, IHoneyComb, ICatalog> catalog, IDictionary<string, byte[]> memory) : this(
            name,
            comb => comb,
            catalog,
            memory
        )
        { }

        /// <summary>
        /// A hive that lives in memory.
        /// With this ctor, this hive's contents will live only as long as this instance
        /// lives.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="catalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        public RamHive(string name, Func<string, IHoneyComb, ICatalog> catalog) : this(
            name,
            comb => comb,
            catalog,
            new Dictionary<string, byte[]>()
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
        public RamHive(string name, Func<IHoneyComb, IHoneyComb> comb, Func<string, IHoneyComb, ICatalog> catalog, IDictionary<string, byte[]> memory)
        {
            this.name = name;
            this.comb = comb;
            this.catalog = catalog;
            this.memory = memory;
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return
                new Mapped<string, IHoneyComb>(
                    id => this.comb(new RamComb(id, this.memory)),
                    this.catalog(this.name, HQ()).List(xpath)
                );
        }

        public IHoneyComb HQ()
        {
            return
                this.comb(
                    new RamComb($"{this.name}{Path.DirectorySeparatorChar}HQ", this.memory)
                );
        }

        public string Name()
        {
            return this.name;
        }
    }
}
