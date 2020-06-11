using System;
using System.Collections.Generic;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Knowledge which exists in memory.
    /// </summary>
    public sealed class RamKnowledge : IKnowledge
    {
        private readonly IList<string> contents;

        /// <summary>
        /// Knowledge which exists in memory.
        /// </summary>
        public RamKnowledge(IDictionary<string, string> contents)
        {
            this.contents = new List<string>();
        }

        public IList<string> Contents()
        {
            throw new NotImplementedException();
        }

        public void Forget(string name)
        {
            throw new NotImplementedException();
        }

        public bool Has(string name)
        {
            throw new NotImplementedException();
        }

        public void Introduce(string name)
        {
            throw new NotImplementedException();
        }
    }
}
