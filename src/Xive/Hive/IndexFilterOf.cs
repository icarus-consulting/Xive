using System;

namespace Xive.Hive
{
    /// <summary>
    /// A filter which looks at props.
    /// </summary>
    public sealed class IndexFilterOf : IIndexFilter
    {
        private readonly Func<IProps, bool> matches;

        /// <summary>
        /// A filter which looks at props.
        /// </summary>
        public IndexFilterOf(Func<IProps, bool> matches)
        {
            this.matches = matches;
        }

        public bool Matches(IProps props)
        {
            return this.matches(props);
        }
    }
}
