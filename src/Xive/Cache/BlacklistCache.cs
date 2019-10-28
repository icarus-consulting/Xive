﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.Func;
using Yaapii.Atoms.List;
using Yaapii.Atoms.Scalar;

namespace Xive.Hive
{
    /// <summary>
    /// A cache that accepts a blacklist and only caches if the requested item is not in the list.
    /// </summary>
    public sealed class BlacklistCache : ICache
    {
        private readonly IScalar<string[]> patterns;
        private readonly StickyFunc<string, bool> blacklisted;
        private readonly ICache origin;

        /// <summary>
        /// A cache that accepts a blacklist and only caches if the requested item is not in the list.
        /// </summary>
        public BlacklistCache(params string[] blacklisted) : this(
            new ListOf<string>(blacklisted),
            new SimpleCache()
        )
        { }

        /// <summary>
        /// A cache that accepts a blacklist and only caches if the requested item is not in the list.
        /// </summary>
        public BlacklistCache(IList<string> blacklist, ICache origin)
        {
            this.patterns =
                new Sticky<string[]>(() =>
                {
                    var compiled = new string[blacklist.Count];
                    for (int i = 0; i < blacklist.Count; i++)
                    {
                        compiled[i] = Regex.Escape(blacklist[i].ToLower()).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                    }
                    return compiled;
                });
            this.blacklisted = new StickyFunc<string, bool>((name) => this.IsBlacklisted(name));
            this.origin = origin;
        }

        public MemoryStream Binary(string name, Func<MemoryStream> ifAbsent)
        {
            MemoryStream result;
            if(!this.blacklisted.Invoke(name))
            {
                result = this.origin.Binary(name, ifAbsent);
            }
            else
            {
                result = ifAbsent();
            }
            return result;
        }

        public void Remove(string name)
        {
            this.origin.Remove(name);
        }

        public XNode Xml(string name, Func<XNode> ifAbsent)
        {
            XNode result;
            if (!this.blacklisted.Invoke(name))
            {
                result = this.origin.Xml(name, ifAbsent);
            }
            else
            {
                result = ifAbsent();
            }
            return result;
        }

        public bool Has(string name)
        {
            return this.origin.Has(name);
        }

        private bool IsBlacklisted(string name)
        {
            var result = false;
            var patterns = this.patterns.Value();
            for (var i = 0; i < this.patterns.Value().Length; i++)
            {
                if (Regex.IsMatch(name, patterns[i], RegexOptions.IgnoreCase))
                {
                    result = true;
                    break;
                }
            }
            return result;

        }
    }
}