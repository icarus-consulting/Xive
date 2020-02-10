using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xive.Hive;
using Yaapii.Atoms;
using Yaapii.Atoms.Enumerable;
using Yaapii.Atoms.Func;
using Yaapii.Atoms.Scalar;

namespace Xive.Mnemonic
{
    /// <summary>
    /// Memory which is cached.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CachedMemory<T> : IMemory<T>
    {
        private readonly IMemory<T> cache;
        private readonly BiFuncOf<string, T, bool> skip;
        private readonly IMemory<T> origin;
        private readonly Sticky<string[]> patterns;

        /// <summary>
        /// Memory which is cached.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public CachedMemory(IMemory<T> origin, int maxSize, Func<T, bool> checkEmpty, IEnumerable<string> blacklist) : this(
            origin,
            checkEmpty,
            maxSize,
            new Sticky<IList<string>>(() =>
            {
                return
                    new List<string>(
                        new Mapped<string, string>(
                            (entry) => new Normalized(entry).AsString(),
                            blacklist
                        )
                    );
            })
        )
        { }

        /// <summary>
        /// Memory which is cached.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal CachedMemory(IMemory<T> origin, Func<T, bool> checkEmpty, int maxSize, IScalar<IList<string>> blacklist)
        {
            this.cache = new VersatileRam<T>(checkEmpty);
            this.origin = origin;
            this.patterns =
                new Sticky<string[]>(() =>
                {
                    var blacks = blacklist.Value();
                    var compiled = new string[blacks.Count];
                    for (int i = 0; i < blacks.Count; i++)
                    {
                        compiled[i] = Regex.Escape(blacklist.Value()[i].ToLower()).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                    }
                    return compiled;
                });
            this.skip = new BiFuncOf<string, T, bool>((name, content) =>
            {
                var skip = false;
                name = new Normalized(name).AsString();
                if (content is Stream && (content as Stream).Length > maxSize)
                {
                    skip = true;
                }
                if (IsBlacklisted(name))
                {
                    skip = true;
                }
                return skip;
            });
        }

        public T Content(string name, Func<T> ifAbsent)
        {
            T result;
            if (!this.cache.Knows(name))
            {
                result = this.origin.Content(name, () => ifAbsent());
            }
            else
            {
                result =
                    this.cache.Content(
                        name,
                        () => this.origin.Content(name, () => ifAbsent())
                    );
            }
            if (!this.cache.Knows(name) && !this.skip.Invoke(name, result))
            {
                this.cache.Update(name, result);
            }
            return result;
        }


        public IEnumerable<string> Knowledge()
        {
            return this.origin.Knowledge();
        }

        public bool Knows(string name)
        {
            return this.origin.Knows(name);
        }

        public void Update(string name, T content)
        {
            if (!this.skip.Invoke(name, content))
            {
                this.cache.Update(name, content);
            }
            this.origin.Update(name, content);
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
