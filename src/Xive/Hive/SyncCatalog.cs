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
using Xive.Xocument;
using Yaapii.Atoms;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Error;
using Yaapii.Atoms.Scalar;
using Yaapii.Xambly;

namespace Xive.Hive
{
    /// <summary>
    /// A catalog to manage a list of combs in a hive processwide exclusively.
    /// </summary>
    public sealed class SyncCatalog : ICatalog
    {
        private readonly IScalar<IHoneyComb> hq;
        private readonly IScalar<string> itemName;
        private readonly Func<ICell, IXocument> xocument;
        private readonly ISyncValve syncValve;

        /// <summary>
        /// A catalog to manage a list of combs in a hive processwide exclusively.
        /// </summary>
        public SyncCatalog(IHive hive) : this(hive, new ProcessSyncValve())
        { }

        /// <summary>
        /// A catalog to manage a list of combs in a hive processwide exclusively.
        /// </summary>
        public SyncCatalog(IHive hive, ISyncValve syncValve) : this(
            new ScalarOf<string>(() => hive.Scope()),
            new ScalarOf<IHoneyComb>(() => hive.HQ()),
            cell => new CellXocument(cell, "catalog"),
            syncValve
        )
        { }

        /// <summary>
        /// A catalog to manage a list of combs in a hive processwide exclusively.
        /// </summary>
        public SyncCatalog(string itemName, IHoneyComb hq, ISyncValve syncValve) : this(itemName, hq, cell => new CellXocument(cell, "catalog"), syncValve)
        { }

        /// <summary>
        /// A catalog to manage a list of combs in a hive processwide exclusively.
        /// </summary>
        public SyncCatalog(string itemName, IHoneyComb hq, Func<ICell, IXocument> xocument, ISyncValve syncValve) : this(
            new ScalarOf<string>(itemName),
            new ScalarOf<IHoneyComb>(hq),
            xocument,
            syncValve
        )
        { }

        /// <summary>
        /// A catalog to manage a list of combs in a hive processwide exclusively.
        /// </summary>
        internal SyncCatalog(IScalar<string> itemName, IScalar<IHoneyComb> hq, Func<ICell, IXocument> xocument, ISyncValve syncValve)
        {
            this.hq = hq;
            this.itemName = itemName;
            this.xocument = xocument;
            this.syncValve = syncValve;
        }

        public void Update(string id, IEnumerable<IDirective> content)
        {
            if (!Exists(id))
            {
                Create(id);
            }

            using (var xoc = this.Xocument())
            {
                xoc.Modify(
                    new Directives()
                        .Xpath($"/catalog/{this.itemName.Value()}[@id='{id}']")
                        .Append(content)
                );
            }
        }

        public IEnumerable<string> List(string xpath)
        {
            using (var xoc = this.Xocument())
            {
                return xoc.Values($"//{this.itemName.Value()}[{xpath}]/@id");
            }
        }

        public void Remove(string id)
        {
            new FailWhen(
                () => !Exists(id),
                new InvalidOperationException($"{this.itemName.Value()} '{id}' does not exist")
            ).Go();

            using (var xoc = this.Xocument())
            {
                xoc.Modify(
                    new Directives()
                        .Xpath($"/catalog/{this.itemName.Value()}[@id='{id}']")
                        .Remove()
                );
            }
        }

        public void Create(string id)
        {
            using (var xoc = this.Xocument())
            {
                if (xoc.Nodes($"/catalog/{this.itemName.Value()}[@id='{id}']").Count == 0)
                {
                    xoc.Modify(
                        new Directives()
                            .Xpath("/catalog")
                            .Add(this.itemName.Value())
                            .Attr("id", id)
                    );
                }
            }
        }

        private bool Exists(string id)
        {
            using (var xoc = Xocument())
            {
                return xoc.Nodes($"/catalog/{this.itemName.Value()}[@id='{id}']").Count > 0;
            }
        }

        private IXocument Xocument()
        {
            var hq = this.hq.Value();
            var name = $"{hq.Name()}/catalog.xml";
            return 
                new SyncXocument(
                    name, 
                    this.hq.Value().Xocument("catalog.xml"), 
                    this.syncValve
                );
        }
    }
}
