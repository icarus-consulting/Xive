using System.IO;
using Xive.Mnemonic;
using Yaapii.Atoms.Scalar;

namespace Xive.Xocument
{
    /// <summary>
    /// A xocument stored in a file.
    /// </summary>
    public sealed class FileXocument : XocumentEnvelope
    {
        /// <summary>
        /// A xocument stored in a file.
        /// </summary>
        public FileXocument(string path) : base(
            new Sticky<IXocument>(() =>
            {
                string root = Path.GetDirectoryName(path);
                var name = Path.GetFileName(path);
                return 
                    new MemorizedXocument(
                        name,
                        new FileMemories(root)
                    );
            })
        )
        { }
    }
}
