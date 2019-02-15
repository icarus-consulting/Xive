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
using Xive.Hive;

namespace Xive.Farm
{
    /// <summary>
    /// A farm which lives in memory.
    /// </summary>
    public sealed class RamFarm : IFarm
    {
        private readonly Func<string, IHive> hive;
        private readonly IDictionary<string, IDictionary<string, byte[]>> farmMemory;

        /// <summary>
        /// A farm which lives in memory. The contents will live as long as this instance lives.
        /// </summary>
        public RamFarm() : this(new Dictionary<string, IDictionary<string, byte[]>>())
        { }

        /// <summary>
        /// A farm which lives in memory. The contents will live as long as this instance lives.
        /// Using this ctor, you can tell the farm, how its hives should build a catalog.
        /// </summary>
        public RamFarm(Func<string, IHoneyComb, ICatalog> catalog) : this(comb => comb, hive => hive, catalog, new Dictionary<string, IDictionary<string, byte[]>>())
        { }

        /// <summary>
        /// A farm which lives in memory. The contents will live as long as the injected Dictionary lives.
        /// </summary>
        public RamFarm(IDictionary<string, IDictionary<string, byte[]>> memory) : this(hiveName =>
            {
                if (!memory.ContainsKey(hiveName))
                {
                    memory.Add(hiveName, new Dictionary<string, byte[]>());
                }
                return new RamHive(hiveName, comb => comb, (name, hq) => new Catalog(hiveName, hq), memory[hiveName]);
            },
            memory
        )
        { }

        /// <summary>
        /// A farm which lives in memory. The contents will live as long as the injected Dictionary lives.
        /// Using this ctor, this farm is rather dumb: You must tell it how to build a catalog.
        /// You can use combWrap and/or hiveWrap to wrap if necessary.
        /// </summary>
        public RamFarm(Func<IHive, IHive> hiveWrap, Func<IHoneyComb, IHoneyComb> combWrap, Func<string, IHoneyComb, ICatalog> catalog, IDictionary<string, IDictionary<string, byte[]>> memory) : this(hiveName =>
             {
                 if (!memory.ContainsKey(hiveName))
                 {
                     memory.Add(hiveName, new Dictionary<string, byte[]>());
                 }
                 return hiveWrap(new RamHive(hiveName, comb => combWrap(comb), catalog, memory[hiveName]));
             },
            memory
        )
        { }

        internal RamFarm(Func<string, IHive> hive, IDictionary<string, IDictionary<string, byte[]>> rawMemory)
        {
            this.hive = hive;
            this.farmMemory = rawMemory;
        }

        public IHive Hive(string name)
        {
            return this.hive(name);
        }
    }
}
