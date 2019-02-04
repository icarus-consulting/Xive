using System;
using System.Collections.Generic;
using System.Text;
using Yaapii.Atoms;

namespace Xive.Cell
{
    /// <summary>
    /// A fake cell.
    /// </summary>
    public sealed class FkCell : ICell
    {
        private readonly Func<byte[]> content;
        private readonly Action<IInput> update;

        /// <summary>
        /// A fake cell.
        /// </summary>
        public FkCell(Func<byte[]> content, Action<IInput> update)
        {
            this.content = content;
            this.update = update;
        }

        public byte[] Content()
        {
            return this.content();
        }

        public void Dispose()
        {
            
        }

        public void Update(IInput content)
        {
            this.update(content);
        }
    }
}
