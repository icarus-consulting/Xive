using System;

namespace Xive.Mnemonic
{
    /// <summary>
    /// A cache for data.
    /// </summary>
    public interface ICache<TData>
    {
        void Remove(string name);
        void Update(string name, Func<TData> ifAbsent, Func<TData> ifExists);
        TData Content(string name, Func<TData> ifAbsent);
    }
}
