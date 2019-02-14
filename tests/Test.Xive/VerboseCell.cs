using System;
using System.Collections.Generic;
using System.Text;
using Yaapii.Atoms;

namespace Xive.Test
{
    /// <summary>
    /// A cell that reports when its methods are called.
    /// </summary>
    public sealed class VerboseCell : ICell
    {
        private readonly Action reportContent;
        private readonly Action reportUpdate;
        private readonly Action reportDispose;
        private readonly ICell origin;

        /// <summary>
        /// A cell that reports when its methods are called.
        /// </summary>
        public VerboseCell(ICell origin, Action content, Action update, Action dispose)
        {
            this.reportContent = content;
            this.reportUpdate = update;
            this.reportDispose = dispose;
            this.origin = origin;
        }

        public byte[] Content()
        {
            this.reportContent();
            return this.Content();
        }

        public void Dispose()
        {
            this.reportDispose();
            this.origin.Dispose();
        }

        public void Update(IInput content)
        {
            this.reportUpdate();
            this.origin.Update(content);
        }
    }
}
