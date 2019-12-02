using System;
using System.Collections.Generic;
using System.Text;
using Yaapii.Atoms;

namespace Xive
{
    public sealed class Normalized : IText
    {
        private readonly Lazy<string> name;

        public Normalized(string name)
        {
            this.name = new Lazy<string>(() => name.Replace('\\', '/').ToLower());
        }

        public string AsString()
        {
            return this.name.Value;
        }

        public bool Equals(IText other)
        {
            return this.name.Value.Equals(other.AsString());
        }
    }
}
