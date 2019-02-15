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
using Yaapii.Atoms;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Scalar;

namespace Xive.Hive
{
    /// <summary>
    /// A simple hive.
    /// This hive is rather dumb: You need to tell it how to build a comb and how to build a catalog.
    /// </summary>
    public class SimpleHive : IHive
    {
        private readonly IText name;
        private readonly Func<string, IHoneyComb> comb;
        private readonly IScalar<ICatalog> catalog;

        /// <summary>
        /// A simple hive.
        /// Using this ctor, this hive is rather dumb: You need to tell it how to build a comb.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="comb">How to build a comb: (combName) => new SomeCatalog(...)</param>
        public SimpleHive(string name, Func<string, IHoneyComb> comb) : this(
            name,
            comb,
            (hiveName, catalogComb) => new Catalog(hiveName, catalogComb)
        )
        { }

        /// <summary>
        /// A simple hive.
        /// Using this ctor, this hive is rather dumb: You need to tell it how to build a comb and how to build a catalog.
        /// </summary>
        /// <param name="name">Unique name of the hive</param>
        /// <param name="comb">How to build a comb: (combName) => new SomeCatalog(...)</param>
        /// <param name="hiveCatalog">How the hive should build its catalog: (hiveName, comb) => new SomeCatalog(hiveName, comb)</param>
        public SimpleHive(string name, Func<string, IHoneyComb> comb, Func<string, IHoneyComb, ICatalog> catalog)
        {
            this.name = new Coordinate(name);
            this.comb = comb;
            this.catalog = new StickyScalar<ICatalog>(() => catalog(name, HQ()));
        }

        public IHoneyComb HQ()
        {
            return this.comb($"{this.name.AsString()}{Path.DirectorySeparatorChar}HQ");
        }

        public IEnumerable<IHoneyComb> Combs(string xpath)
        {
            return
                new Mapped<string, IHoneyComb>(
                    comb => 
                    {
                        return this.comb($"{this.name.AsString()}{Path.DirectorySeparatorChar}{comb}");
                    },
                    this.catalog.Value().List(xpath)
                );
        }

        public string Name()
        {
            return this.name.AsString();
        }
    }
}
