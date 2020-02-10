using System.IO;
using Xive.Cache;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which is cached in memory.
    /// </summary>
    public sealed class CachedCell : ICell
    {
        private readonly ICell origin;
        private readonly IMemories mem;

        /// <summary>
        /// A cell which is cached in memory.
        /// </summary>
        public CachedCell(ICell origin, IMemories mem)
        {
            this.origin = origin;
            this.mem = mem;

        }

        public byte[] Content()
        {
            return
                this.mem
                    .Data()
                    .Content(
                        this.origin.Name(),
                        () => new MemoryStream(this.origin.Content())
                    ).ToArray();
        }

        public string Name()
        {
            return this.origin.Name();
        }

        public void Update(IInput content)
        {
            var stream = content.Stream();
            if (stream.Length > 0)
            {
                var memory = new MemoryStream();
                content.Stream().CopyTo(memory);
                memory.Seek(0, SeekOrigin.Begin);
                content.Stream().Seek(0, SeekOrigin.Begin);
                this.mem.Data().Update(this.origin.Name(), memory);
            }
            else
            {
                this.mem
                    .Data()
                    .Update(this.origin.Name(), new MemoryStream()
                );
            }
            this.origin.Update(
                new InputOf(
                    this.mem
                        .Data()
                        .Content(this.origin.Name(), () => new MemoryStream())
                    )
                );
        }

        public void Dispose()
        { }
    }
}
