using System.Xml.Linq;

namespace Xive.Mnemonic
{
    /// <summary>
    /// The memories of the xive.
    /// </summary>
    public interface IMnemonic
    {
        /// <summary>
        /// Props Memory - key: value
        /// they are assigned to a scope and an entity with a specific id.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IProps Props(string scope, string id);

        /// <summary>
        /// XML Memory.
        /// </summary>
        /// <returns></returns>
        IMemory<XNode> XML();

        /// <summary>
        /// Data Memory.
        /// </summary>
        /// <returns></returns>
        IMemory<byte[]> Data();
    }

    /// <summary>
    /// The memories of the xive.
    /// </summary>
    public interface IMnemonic2
    {
        /// <summary>
        /// Props Memory - key: value
        /// they are assigned to a scope and an entity with a specific id.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IProps Props(string scope, string id);

        IContents Contents();
    }
}
