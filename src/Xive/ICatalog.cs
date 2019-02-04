using System.Collections.Generic;
using Yaapii.Xambly;

namespace Xive
{
    public interface ICatalog
    {
        void Create(string id);
        void Update(string id, IEnumerable<IDirective> content);
        IEnumerable<string> List(string xpath);
        void Remove(string id);
    }
}
