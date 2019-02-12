using System;
using System.Collections.Generic;
using System.Text;
using Yaapii.Atoms;

namespace Xive.Test
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
        public FkCell() : this(content => { }, () => new byte[0])
        { }

        /// <summary>
        /// A fake cell.
        /// </summary>
        public FkCell(Action<IInput> update, Func<byte[]> content)
        {
            this.content = content;
            this.update = update;
        }

        public byte[] Content()
        {
            return this.content();
        }

        public void Update(IInput content)
        {
            this.update(content);
        }

        public void Dispose()
        { }
    }
}
