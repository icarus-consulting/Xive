﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using Yaapii.Atoms;
using Yaapii.Atoms.Func;

namespace Xive
{
    /// <summary>
    /// A coordinate. A path to a cell consists of multiple coordinates.
    /// </summary>
    public sealed class Coordinate : IText
    {
        private readonly string text;
        private readonly IFunc<string,string> validated;

        /// <summary>
        /// A coordinate. A path to a cell consists of multiple coordinates.
        /// </summary>
        /// <param name="text"></param>
        public Coordinate(string text)
        {
            this.text = text;
            this.validated = new StickyFunc<string, string>(txt => Validated(txt));
        }

        public string AsString()
        {
            return this.validated.Invoke(this.text);
        }

        public bool Equals(IText other)
        {
            return other.AsString().Equals(this.AsString());
        }

        private string Validated(string text)
        {
            string regexSearch = new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

            if(r.Matches(text).Count > 0)
            {
                throw new ArgumentException($"'{text}' is not a valid coordinate, it contains illegal characters. Illegal characters are: $',\\r,\\n,{String.Join(",", Path.GetInvalidPathChars())}'");
            }
            return text;
        }
    }
}
