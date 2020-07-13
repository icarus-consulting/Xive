using System.Text;
using System.Xml.Linq;
using Yaapii.Atoms;
using Yaapii.Atoms.Bytes;
using Yaapii.Atoms.Scalar;

namespace Xive
{
    /// <summary>
    /// XNode as Bytes, encoded with UTF8.
    /// </summary>
    public sealed class XNodeBytes : IBytes
    {
        private readonly ScalarOf<byte[]> bytes;

        /// <summary>
        /// XNode as Bytes, encoded with UTF8.
        /// </summary>
        public XNodeBytes(XNode node)
        {
            this.bytes =
                new ScalarOf<byte[]>(() =>
                {
                    return
                        new BytesOf(
                            Encoding.UTF8.GetBytes(node.ToString())
                        ).AsBytes();
                });
        }

        public byte[] AsBytes()
        {
            return this.bytes.Value();
        }
    }
}
