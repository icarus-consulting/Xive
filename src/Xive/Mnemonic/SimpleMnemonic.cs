using System.Collections.Concurrent;
using Xive.Mnemonic;
using Xive.Props;

namespace Xive.Cache
{
    /// <summary>
    /// Simple Memories.
    /// </summary>
    public sealed class SimpleMnemonic : IMnemonic
    {
        private readonly IContents contentMem;

        /// <summary>
        /// Simple Memories.
        /// </summary>
        public SimpleMnemonic(IContents contents)
        {
            this.contentMem = contents;
        }

        public IProps Props(string scope, string id)
        {
            return new SandboxProps(this.contentMem, scope, id);
        }

        public IContents Contents()
        {
            return this.contentMem;
        }
    }
}
