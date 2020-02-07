using System;
using System.Collections.Generic;
using System.Text;

namespace Xive.Index
{
    public sealed class PersistentIndex : IIndex
    {
        public PersistentIndex()
        {

        }

        public void Add(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Find(params IIndexFilter[] filters)
        {
            throw new NotImplementedException();
        }

        public bool Has(string id)
        {
            throw new NotImplementedException();
        }

        public void Remove(string id)
        {
            throw new NotImplementedException();
        }
    }
}
