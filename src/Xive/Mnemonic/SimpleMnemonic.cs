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
        private readonly ConcurrentDictionary<string, IProps> propsMem;

        /// <summary>
        /// Simple Memories.
        /// </summary>
        public SimpleMnemonic(IContents contents)
        {
            this.contentMem = contents;
            this.propsMem = new ConcurrentDictionary<string, IProps>();
        }

        public IProps Props(string scope, string id)
        {
            return
                this.propsMem.GetOrAdd(
                    $"{scope}/{id}",
                    key =>
                    {
                        return new SandboxProps2(this, scope, id);
                    }
                );
        }

        public IContents Contents()
        {
            return this.contentMem;
        }
    }
}
