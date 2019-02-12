using System;
using System.Collections.Generic;
using Yaapii.Atoms;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Error;
using Yaapii.Atoms.Scalar;
using Yaapii.Xambly;
using Xive.Xocument;

namespace Xive.Hive
{
    /// <summary>
    /// A catalog to manage a list of combs in a hive.
    /// </summary>
    public sealed class Catalog : ICatalog
    {
        private readonly IScalar<IComb> hq;
        private readonly IScalar<string> itemName;
        private readonly Func<ICell, IXocument> xocument;

        /// <summary>
        /// A catalog to manage a list of combs in a hive.
        /// </summary>
        public Catalog(IHive hive) : this(
            new ScalarOf<string>(() => hive.Name()),
            new ScalarOf<IComb>(() => hive.HQ()),
            cell => new CellXocument(cell, "catalog", "0.0.1")
        )
        { }

        /// <summary>
        /// A catalog to manage a list of combs in a hive.
        /// </summary>
        public Catalog(string itemName, IComb hq) : this(itemName, hq, cell => new CellXocument(cell, "catalog", "0.0.1"))
        { }

        /// <summary>
        /// A catalog to manage a list of combs in a hive.
        /// </summary>
        public Catalog(string itemName, IComb hq, Func<ICell, IXocument> xocument) : this(
            new ScalarOf<string>(itemName),
            new ScalarOf<IComb>(hq),
            xocument
        )
        { }

        /// <summary>
        /// A catalog to manage a list of combs in a hive.
        /// </summary>
        internal Catalog(IScalar<string> itemName, IScalar<IComb> hq, Func<ICell, IXocument> xocument)
        {
            this.hq = hq;
            this.itemName = itemName;
            this.xocument = xocument;
        }

        public void Update(string id, IEnumerable<IDirective> content)
        {
            if(!Exists(id))
            {
                Create(id);
            }

            using (var cell = this.Cell())
            {
                this.xocument(cell).Modify(
                    new Directives()
                        .Xpath($"/catalog/{this.itemName.Value()}[@id='{id}']")
                        .Append(content)
                );
            }
        }

        public IEnumerable<string> List(string xpath)
        {
            using (var cell = this.Cell())
            {
                var xoc = this.xocument(cell);
                return xoc.Values($"//{this.itemName.Value()}[{xpath}]/@id");
            }
        }

        public void Remove(string id)
        {
            new FailWhen(
                () => !Exists(id),
                new InvalidOperationException($"{this.itemName.Value()} '{id}' does not exist")
            ).Go();

            using (var cell = this.Cell())
            {
                this.xocument(cell).Modify(
                    new Directives()
                        .Xpath($"/catalog/{this.itemName.Value()}[@id='{id}']")
                        .Remove()
                );
            }
        }

        public void Create(string id)
        {
            using (var cell = this.Cell())
            {
                this.xocument(cell)
                    .Modify(
                        new Directives()
                            .Xpath("/catalog")
                            .Append(
                                new EnumerableOf<IDirective>(
                                    new AddIfAttributeDirective(this.itemName.Value(), "id", id)
                                )
                            )
                            .Attr("id", id)
                    );
            }
        }

        private bool Exists(string id)
        {
            return this.List($"@id='{id}'").GetEnumerator().MoveNext();
        }

        private ICell Cell()
        {
            return this.hq.Value().Cell("catalog.xml");
        }
    }
}
