﻿//MIT License

//Copyright (c) 2022 ICARUS Consulting GmbH

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
using System.Text;
using Xive.Cell;
using Xive.Comb;
using Xive.Mnemonic;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Hive
{
    /// <summary>
    /// The index of a xive, realized as a simple text document.
    /// </summary>
    public sealed class TextIndex : IIndex
    {
        private readonly string scope;
        private readonly IMnemonic mem;
        private readonly List<string> idCache;

        /// <summary>
        /// The index of a xive, realized as a simple text document.
        /// </summary>
        public TextIndex(string scope, IMnemonic mem)
        {
            this.mem = mem;
            this.scope = scope;
            this.idCache = new List<string>();
        }

        public IHoneyComb Add(string id)
        {
            if (id.Contains("\r"))
            {
                throw new ArgumentException($"Cannot use id with character \\r inside. This is reserved for internal usage.");
            }
            lock (idCache)
            {
                var cell = Cell();
                if (idCache.Count == 0)
                {
                    idCache.AddRange(IdsFromCell());
                }
                if (!idCache.Contains(id))
                {
                    idCache.Add(id);
                }
                cell.Update(new InputOf(string.Join(";", idCache)));
                return new MemorizedComb($"{scope}/{id}", this.mem);
            }
        }

        public IList<IHoneyComb> List(params IHiveFilter[] filters)
        {
            var filtered = new List<IHoneyComb>();
            lock (idCache)
            {
                if (idCache.Count == 0)
                {
                    idCache.AddRange(IdsFromCell());
                }
            }
            return
                new ListOf<IHoneyComb>(
                    new Yaapii.Atoms.Enumerable.Mapped<string, IHoneyComb>(id =>
                        new MemorizedComb(
                            $"{scope}/{id}",
                            this.mem
                        ),
                        new Filtered<string>(id =>
                            filters.Length == 0 ||
                            new And(
                                new Yaapii.Atoms.Enumerable.Mapped<IHiveFilter, bool>(
                                    filter => filter.Matches(this.mem.Props(scope, id)),
                                    filters
                                )
                            ).Value(),
                            idCache.ToArray()
                        )
                    )
                );
        }

        public IHoneyComb Comb(string id)
        {
            if (!this.Has(id))
            {
                throw new ArgumentException($"Cannot find unknown id '{id}'.");
            }
            return new MemorizedComb($"{scope}/{id}", this.mem);
        }

        public bool Has(string id)
        {
            lock (idCache)
            {
                if (idCache.Count == 0)
                {
                    idCache.AddRange(IdsFromCell());
                }
                return idCache.Contains(id);
            }
        }

        public void Remove(string id)
        {
            var prefix = new Normalized($"{scope}/{id}").AsString();
            foreach (var data in this.mem.Contents().Knowledge(prefix))
            {
                if (data.StartsWith(prefix))
                {
                    this.mem.Contents().UpdateBytes(data, new byte[0]);
                }
            }
            lock (idCache)
            {
                if (idCache.Count == 0)
                {
                    idCache.AddRange(IdsFromCell());
                }
                idCache.Remove(id);
                Cell().Update(new InputOf(string.Join(";", idCache)));
            }
        }

        private IEnumerable<string> IdsFromCell()
        {
            return
                new TextOf(
                    new BytesOf(
                        Cell().Content()
                    ),
                    Encoding.UTF8
                ).AsString()
                .Split(
                    new char[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries
                );
        }

        private ICell Cell()
        {
            return new MemorizedCell($"{scope}/hq/catalog.cat", this.mem);
        }
    }
}
