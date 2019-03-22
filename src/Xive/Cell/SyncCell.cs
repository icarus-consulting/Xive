using System.Collections.Generic;
using Yaapii.Atoms;
using Yaapii.Atoms.Scalar;

namespace Xive.Cell
{
    /// <summary>
    /// A cell that is threadsafe.
    /// </summary>
    public sealed class SyncCell : ICell
    {
        private readonly IScalar<IDictionary<string, bool>> locks;
        private readonly ICell origin;

        public SyncCell(ICell origin) : this(
            origin,
            new SyncScalar<IDictionary<string, bool>>(
                () => new Dictionary<string, bool>()
                {
                    { origin.Name(), false }
                },
                origin
            )
        )
        { }

        public SyncCell(ICell origin, IScalar<IDictionary<string, bool>> locks)
        {
            this.locks = locks;
            this.origin = origin;
        }

        public string Name()
        {
            return origin.Name();
        }

        public byte[] Content()
        {
            return origin.Content();
        }

        public void Dispose()
        {
            this.origin.Dispose();
        }

        public void Update(IInput content)
        {
            origin.Update(content);
        }
    }
}
