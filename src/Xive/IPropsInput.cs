using System.Collections.Generic;

namespace Xive
{
    public interface IPropsInput
    {
        IDictionary<string, IList<string>> Apply(IDictionary<string, IList<string>> props);
    }
}
