using System;
using System.Collections.Generic;
using System.Text;
using Yaapii.Atoms;

namespace Xive
{
    /// <summary>
    /// A text with only forward slashes instead of backslashes and only lower-cased
    /// </summary>
    public sealed class Normalized : IText
    {
        private readonly Lazy<string> name;

        /// <summary>
        /// A text with only forward slashes instead of backslashes and only lower-cased
        /// </summary>
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
