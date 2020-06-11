using System;
using System.Collections.Generic;
using System.Text;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Knowledge which is dead.
    /// It will not reject anything, it just does nothing.
    /// </summary>
    public sealed class DeadKnowledge : IKnowledge
    {
        /// <summary>
        /// Knowledge which is dead.
        /// It will not reject anything, it just does nothing.
        /// </summary>
        public DeadKnowledge()
        { }

        public IList<string> Contents()
        {
            return new List<string>();
        }

        public void Forget(string name)
        { }

        public bool Has(string name)
        {
            return false;
        }

        public void Introduce(string name)
        { }
    }
}
