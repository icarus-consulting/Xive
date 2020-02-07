using System.Collections.Generic;

namespace Xive
{
    public interface IProps
    {
        IProps Refined(string prop, params string[] value);
        IProps Refined(params IPropsInput[] props);
        string Value(string prop, string def = "");
        IList<string> Values(string prop);
        IList<string> Names();
    }
}
