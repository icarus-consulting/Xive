using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive
{
    /// <summary>
    /// A text with only forward slashes instead of backslashes and only lower-cased
    /// </summary>
    public sealed class Normalized : IText
    {
        private readonly Sticky<string> name;

        /// <summary>
        /// A text with only forward slashes instead of backslashes and only lower-cased
        /// </summary>
        public Normalized(string name)
        {
            this.name = new Sticky<string>(() => name.Replace('\\', '/').ToLower());
        }

        public string AsString()
        {
            return this.name.Value();
        }

        public bool Equals(IText other)
        {
            return this.name.Value().Equals(other.AsString());
        }
    }
}
