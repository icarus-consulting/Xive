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

namespace Xive.Mnemonic.Cache
{
    /// <summary>
    /// A fake cache.
    /// </summary>
    public sealed class FkCache<TData> : ICache<TData>
    {
        private readonly Func<string, TData> acquire;
        private readonly Action<string, TData> update;
        private readonly Action<string> remove;
        private readonly Action clear;

        /// <summary>
        /// A fake cache.
        /// </summary>
        public FkCache(Action clear) : this(
            (name) => throw new InvalidOperationException($"Fake cache has not been setup to support getting content"),
            (name, data) => throw new InvalidOperationException($"Fake cache has not been setup to support updates"),
            (name) => throw new InvalidOperationException($"Fake cache has not been setup to support removal"),
            clear
        )
        { }

        /// <summary>
        /// A fake cache.
        /// </summary>
        public FkCache(Action<string> remove) : this(
            (name) => throw new InvalidOperationException($"Fake cache has not been setup to support getting content"),
            (name, data) => throw new InvalidOperationException($"Fake cache has not been setup to support updates"),
            remove,
            () => throw new InvalidOperationException($"Fake cache has not been setup to support clear")
        )
        { }

        /// <summary>
        /// A fake cache.
        /// </summary>
        public FkCache(Action<string, TData> update) : this(
            (name) => throw new InvalidOperationException($"Fake cache has not been setup to support getting content"),
            update,
            (name) => throw new InvalidOperationException($"Fake cache has not been setup to support removal"),
            () => throw new InvalidOperationException($"Fake cache has not been setup to support clear")
        )
        { }

        /// <summary>
        /// A fake cache.
        /// </summary>
        public FkCache(Func<string, TData> acquire) : this(
            acquire,
            (name, data) => throw new InvalidOperationException($"Fake cache has not been setup to support updates"),
            (name) => throw new InvalidOperationException($"Fake cache has not been setup to support removal"),
            () => throw new InvalidOperationException($"Fake cache has not been setup to support clear")
        )
        { }

        /// <summary>
        /// A fake cache.
        /// </summary>
        public FkCache(Func<string, TData> acquire, Action<string, TData> update, Action<string> remove, Action clear)
        {
            this.acquire = acquire;
            this.update = update;
            this.remove = remove;
            this.clear = clear;
        }

        public void Clear()
        {
            this.clear();
        }

        public TData Content(string name, Func<TData> ifAbsent)
        {
            return this.acquire(name);
        }

        public void Remove(string name)
        {
            this.remove(name);
        }

        public void Update(string name, Func<TData> ifAbsent, Func<TData> ifExists)
        {
            this.update(name, ifAbsent());
        }
    }
}
