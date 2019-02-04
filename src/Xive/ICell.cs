using System;
using Yaapii.Atoms;

namespace Xive
{
    /// <summary>
    /// A cell which contains contents as bytes.
    /// </summary>
    public interface ICell : IDisposable
    {
        /// <summary>
        /// The content of the cell.
        /// </summary>
        /// <returns>The content as bytes</returns>
        byte[] Content();

        /// <summary>
        /// Update the cell contents.
        /// </summary>
        /// <param name="content">new contents as bytes</param>
        void Update(IInput content);
    }
}
