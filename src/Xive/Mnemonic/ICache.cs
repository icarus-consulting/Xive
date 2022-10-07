using System;

namespace Xive.Mnemonic
{
    /// <summary>
    /// A cache for data.
    /// </summary>
    public interface ICache<TData>
    {
        /// <summary>
        /// Clear the whole cache
        /// </summary>
        void Clear();

        /// <summary>
        /// The cached content and if no value is in the cache the ifAbsent is called
        /// </summary>
        TData Content(string name, Func<TData> ifAbsent);

        /// <summary>
        /// Remove one item from the cache
        /// </summary>
        void Remove(string name);

        /// <summary>
        /// Update a value in the cache
        /// </summary>
        void Update(string name, Func<TData> ifAbsent, Func<TData> ifExists);
    }
}
