using System;
using System.Collections.Generic;

namespace Xive
{
    /// <summary>
    /// A fake capsule props input.
    /// </summary>
    public sealed class FkPropsInput : IPropsInput
    {
        private readonly Func<IDictionary<string, IList<string>>, IDictionary<string, IList<string>>> apply;

        /// <summary>
        /// A fake capsule props input.
        /// </summary>
        public FkPropsInput(Func<IDictionary<string, IList<string>>, IDictionary<string, IList<string>>> apply)
        {
            this.apply = apply;
        }

        public IDictionary<string, IList<string>> Apply(IDictionary<string, IList<string>> props)
        {
            return this.apply(props);
        }
    }
}
