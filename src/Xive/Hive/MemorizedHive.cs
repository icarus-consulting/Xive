//MIT License

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
using System.Collections.Concurrent;
using Xive.Comb;
using Xive.Mnemonic;
using Yaapii.Atoms;
using Yaapii.Atoms.Error;
using Yaapii.Atoms.Text;

namespace Xive.Hive
{
    /// <summary>
    /// A hive which is memorized in the given memories.
    /// </summary>
    public sealed class MemorizedHive : IHive
    {
        private readonly IText scope;
        private readonly ConcurrentDictionary<string, IIndex> indices;
        private readonly IMnemonic mem;

        /// <summary>
        /// A hive which is memorized in the given memories.
        /// </summary>
        public MemorizedHive(string scope, IMnemonic memories) : this(scope, memories, new ConcurrentDictionary<string, IIndex>())
        { }

        /// <summary>
        /// A hive which is memorized in the given memories.
        /// </summary>
        public MemorizedHive(string scope, IMnemonic memories, ConcurrentDictionary<string, IIndex> indices)
        {
            this.scope = 
                new TextOf(()=>
                {
                    new FailWhen(
                        string.IsNullOrEmpty(scope),
                        new ArgumentException($"Unable to shift memory, because empty scopes are not allowed.")
                    ).Go();
                    return scope;
                });
            this.mem = memories;
            this.indices = indices;
        }

        public IIndex Catalog()
        {
            return this.indices.GetOrAdd(scope.AsString(), new TextIndex(scope.AsString(), this.mem));
        }

        public IHoneyComb Comb(string id, bool createIfAbsent = true)
        {
            if (!Catalog().Has(id))
            {
                if (createIfAbsent)
                {
                    Catalog().Add(id);
                }
                else
                {
                    throw new ArgumentException($"Cannot access a non existing comb with id '{id}'");
                }
            }
            return new MemorizedComb(new Normalized($"{this.scope.AsString()}/{id}").AsString(), this.mem);
        }

        public IHoneyComb HQ()
        {
            return new MemorizedComb(new Normalized($"{this.scope.AsString()}/hq").AsString(), this.mem);
        }

        public string Scope()
        {
            return this.scope.AsString();
        }

        public IHive Shifted(string scope)
        {
            return new MemorizedHive(scope, this.mem, this.indices);
        }
    }
}
