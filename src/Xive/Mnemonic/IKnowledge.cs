using System.Collections.Generic;

namespace Xive.Mnemonic
{
    public interface IKnowledge
    {
        void Introduce(string name);
        bool Has(string name);
        IList<string> Contents();
        void Forget(string name);
    }
}
