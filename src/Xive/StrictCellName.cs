using System;
using System.IO;
using System.Text.RegularExpressions;
using Yaapii.Atoms;

namespace Xive
{
    /// <summary>
    /// A root which does reject invalid chars.
    /// </summary>
    public sealed class StrictCellName : IText
    {
        private readonly string text;

        public StrictCellName(string name)
        {
            this.text = name;
        }

        public string AsString()
        {
            if (this.text.Contains(" ") || this.text.Contains("\r") || this.text.Contains("\n"))
            {
                throw new ArgumentException($"Can't use '{text}' as xml root because it contains whitespaces");
            }
            if (this.text.Contains("&") || this.text.Contains("<") || this.text.Contains(">"))
            {
                throw new ArgumentException($"Can't use '{ text }' as xml root because it contains illegal chars (&,< or >)");
            }
            return text;
        }

        public bool Equals(IText other)
        {
            return this.text.Equals(other.AsString());
        }
    }
}
